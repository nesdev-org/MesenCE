package ca.mesen.android;

import android.app.AlertDialog;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.database.Cursor;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.hardware.input.InputManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.DocumentsContract;
import android.provider.OpenableColumns;
import android.view.Gravity;
import android.view.InputDevice;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import android.widget.ScrollView;
import android.widget.Space;
import android.widget.TextView;
import android.widget.Toast;

import org.libsdl.app.SDLActivity;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;

/**
 * Android shell for MesenCE. The native emulator remains behind SDL, while
 * this activity provides a TV-friendly ROM library and optional touch input.
 */
public final class MesenActivity extends SDLActivity implements InputManager.InputDeviceListener {
    private static final int OPEN_ROM_REQUEST = 1001;
    private static final int OPEN_FOLDER_REQUEST = 1002;
    private static final String PREFS_NAME = "mesence_android";
    private static final String FOLDER_PREFIX = "folder_";
    private static final String ZIP_EXTENSION = ".zip";
    private static final String[] ROM_EXTENSIONS = {
            ".nes", ".fds", ".qd", ".unif", ".unf", ".nsf", ".nsfe", ".studybox",
            ".sfc", ".swc", ".fig", ".smc", ".bs", ".st", ".spc",
            ".gb", ".gbc", ".gbx", ".gbs", ".pce", ".sgx", ".cue", ".hes",
            ".sms", ".gg", ".sg", ".col", ".gba", ".ws", ".wsc", ".pc2"
    };

    // Values match Core/Shared/KeyDefinitions.h.
    private static final int KEY_UP = 24;
    private static final int KEY_DOWN = 26;
    private static final int KEY_LEFT = 23;
    private static final int KEY_RIGHT = 25;
    private static final int KEY_SELECT = 48; // E
    private static final int KEY_START = 61;  // R
    private static final int KEY_A = 67;      // X
    private static final int KEY_B = 69;      // Z
    private static final int KEY_X = 62;      // S
    private static final int KEY_Y = 44;      // A
    private static final int KEY_L = 60;      // Q
    private static final int KEY_R = 66;      // W

    private static final class ConsoleGroup {
        final String id;
        final String title;
        final Set<String> extensions;

        ConsoleGroup(String id, String title, String... extensions) {
            this.id = id;
            this.title = title;
            this.extensions = new HashSet<>(Arrays.asList(extensions));
        }
    }

    private static final class RomEntry {
        final String name;
        final Uri uri;

        RomEntry(String name, Uri uri) {
            this.name = name;
            this.uri = uri;
        }
    }

    private final LinkedHashMap<String, ConsoleGroup> groups = new LinkedHashMap<>();
    private final Map<String, Uri> folderUris = new HashMap<>();
    private final Map<String, LinearLayout> gameLists = new HashMap<>();
    private final Map<String, TextView> folderLabels = new HashMap<>();
    private final ArrayList<View> touchControlViews = new ArrayList<>();
    private SharedPreferences preferences;
    private InputManager inputManager;
    private LinearLayout menuOverlay;
    private TextView menuStatus;
    private Button firstMenuButton;
    private String pendingFolderGroupId;
    private boolean gameRunning;

    private int dp(float value) {
        return (int) (value * getResources().getDisplayMetrics().density + 0.5f);
    }

    private void initConsoleGroups() {
        groups.put("nes", new ConsoleGroup("nes", "Nintendo / NES", ".nes", ".unf", ".unif", ".fds"));
        groups.put("snes", new ConsoleGroup("snes", "Super Nintendo / SNES", ".smc", ".sfc", ".swc", ".bs", ".fig"));
        groups.put("gb", new ConsoleGroup("gb", "Game Boy / Game Boy Color", ".gb", ".gbc"));
        groups.put("gba", new ConsoleGroup("gba", "Game Boy Advance", ".gba"));
        groups.put("pce", new ConsoleGroup("pce", "PC Engine / TurboGrafx", ".pce", ".sgx", ".hes"));
        groups.put("sms", new ConsoleGroup("sms", "Master System / Game Gear", ".sms", ".gg", ".sg"));
        groups.put("ws", new ConsoleGroup("ws", "WonderSwan", ".ws", ".wsc"));
        groups.put("zip", new ConsoleGroup("zip", "Archivos comprimidos / ZIP", ZIP_EXTENSION));
    }

    private Button makeTouchButton(String label, int keyCode) {
        Button button = new Button(this);
        button.setText(label);
        button.setTextSize(12);
        button.setAllCaps(false);
        button.setContentDescription(label);
        button.setFocusable(false);
        button.setMinWidth(0);
        button.setMinHeight(0);
        button.setPadding(0, 0, 0, 0);
        button.setOnTouchListener((view, event) -> {
            switch (event.getActionMasked()) {
                case MotionEvent.ACTION_DOWN:
                case MotionEvent.ACTION_POINTER_DOWN:
                    nativeSetKeyState(keyCode, true);
                    return true;
                case MotionEvent.ACTION_UP:
                case MotionEvent.ACTION_POINTER_UP:
                case MotionEvent.ACTION_CANCEL:
                    nativeSetKeyState(keyCode, false);
                    return true;
                default:
                    return true;
            }
        });
        return button;
    }

    private Button makeMenuButton(String label) {
        Button button = new Button(this);
        button.setText(label);
        button.setTextSize(15);
        button.setAllCaps(false);
        button.setFocusable(true);
        button.setFocusableInTouchMode(true);
        button.setMinHeight(dp(46));
        button.setContentDescription(label);
        return button;
    }

    private LinearLayout makeDPad() {
        LinearLayout dpad = new LinearLayout(this);
        dpad.setOrientation(LinearLayout.VERTICAL);
        dpad.setGravity(Gravity.CENTER);

        LinearLayout upRow = new LinearLayout(this);
        upRow.setGravity(Gravity.CENTER);
        upRow.addView(makeTouchButton("▲", KEY_UP), new LinearLayout.LayoutParams(dp(52), dp(52)));
        dpad.addView(upRow, new LinearLayout.LayoutParams(-2, dp(52)));

        LinearLayout middleRow = new LinearLayout(this);
        middleRow.setGravity(Gravity.CENTER);
        middleRow.addView(makeTouchButton("◀", KEY_LEFT), new LinearLayout.LayoutParams(dp(52), dp(52)));
        Space center = new Space(this);
        middleRow.addView(center, new LinearLayout.LayoutParams(dp(52), dp(52)));
        middleRow.addView(makeTouchButton("▶", KEY_RIGHT), new LinearLayout.LayoutParams(dp(52), dp(52)));
        dpad.addView(middleRow, new LinearLayout.LayoutParams(-2, dp(52)));

        LinearLayout downRow = new LinearLayout(this);
        downRow.setGravity(Gravity.CENTER);
        downRow.addView(makeTouchButton("▼", KEY_DOWN), new LinearLayout.LayoutParams(dp(52), dp(52)));
        dpad.addView(downRow, new LinearLayout.LayoutParams(-2, dp(52)));
        return dpad;
    }

    private LinearLayout makeActionPad() {
        LinearLayout actions = new LinearLayout(this);
        actions.setOrientation(LinearLayout.VERTICAL);
        actions.setGravity(Gravity.CENTER);

        LinearLayout shoulder = new LinearLayout(this);
        shoulder.setGravity(Gravity.CENTER);
        shoulder.addView(makeTouchButton("L", KEY_L), new LinearLayout.LayoutParams(dp(46), dp(42)));
        shoulder.addView(makeTouchButton("X", KEY_X), new LinearLayout.LayoutParams(dp(46), dp(42)));
        shoulder.addView(makeTouchButton("Y", KEY_Y), new LinearLayout.LayoutParams(dp(46), dp(42)));
        shoulder.addView(makeTouchButton("R", KEY_R), new LinearLayout.LayoutParams(dp(46), dp(42)));
        actions.addView(shoulder, new LinearLayout.LayoutParams(-2, dp(42)));

        LinearLayout face = new LinearLayout(this);
        face.setGravity(Gravity.CENTER);
        face.addView(makeTouchButton("B", KEY_B), new LinearLayout.LayoutParams(dp(58), dp(58)));
        face.addView(makeTouchButton("A", KEY_A), new LinearLayout.LayoutParams(dp(58), dp(58)));
        actions.addView(face, new LinearLayout.LayoutParams(-2, dp(58)));

        LinearLayout system = new LinearLayout(this);
        system.setGravity(Gravity.CENTER);
        system.addView(makeTouchButton("Select", KEY_SELECT), new LinearLayout.LayoutParams(dp(78), dp(44)));
        system.addView(makeTouchButton("Start", KEY_START), new LinearLayout.LayoutParams(dp(78), dp(44)));
        actions.addView(system, new LinearLayout.LayoutParams(-2, dp(44)));
        return actions;
    }

    private void addTouchControls() {
        RelativeLayout.LayoutParams dpadParams = new RelativeLayout.LayoutParams(-2, -2);
        dpadParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
        dpadParams.addRule(RelativeLayout.ALIGN_PARENT_START);
        dpadParams.setMargins(dp(10), 0, 0, dp(10));
        View dpad = makeDPad();
        touchControlViews.add(dpad);
        mLayout.addView(dpad, dpadParams);

        RelativeLayout.LayoutParams actionParams = new RelativeLayout.LayoutParams(-2, -2);
        actionParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
        actionParams.addRule(RelativeLayout.ALIGN_PARENT_END);
        actionParams.setMargins(0, 0, dp(10), dp(10));
        View actionPad = makeActionPad();
        touchControlViews.add(actionPad);
        mLayout.addView(actionPad, actionParams);
        setTouchControlsVisible(false);
    }

    private void setTouchControlsVisible(boolean visible) {
        int visibility = visible ? View.VISIBLE : View.GONE;
        for (View view : touchControlViews) {
            view.setVisibility(visibility);
        }
    }

    private TextView makeText(String text, float size, int color) {
        TextView label = new TextView(this);
        label.setText(text);
        label.setTextSize(size);
        label.setTextColor(color);
        return label;
    }

    private void buildLibraryMenu() {
        menuOverlay = new LinearLayout(this);
        menuOverlay.setOrientation(LinearLayout.VERTICAL);
        menuOverlay.setGravity(Gravity.CENTER_HORIZONTAL);
        menuOverlay.setPadding(dp(18), dp(12), dp(18), dp(12));
        menuOverlay.setBackground(new ColorDrawable(Color.argb(248, 10, 12, 15)));
        menuOverlay.setFocusable(true);

        TextView title = makeText("MesenCE · Biblioteca", 22, Color.WHITE);
        title.setGravity(Gravity.CENTER);
        menuOverlay.addView(title, new LinearLayout.LayoutParams(-1, dp(42)));
        TextView help = makeText("Elige una carpeta por consola. Usa el control remoto para moverte y OK para abrir.", 13, Color.LTGRAY);
        help.setGravity(Gravity.CENTER);
        help.setPadding(0, 0, 0, dp(8));
        menuOverlay.addView(help, new LinearLayout.LayoutParams(-1, -2));

        menuStatus = makeText("Selecciona una carpeta para empezar", 13, Color.rgb(180, 220, 180));
        menuStatus.setGravity(Gravity.CENTER);
        menuOverlay.addView(menuStatus, new LinearLayout.LayoutParams(-1, dp(30)));

        ScrollView scroll = new ScrollView(this);
        scroll.setFillViewport(true);
        LinearLayout library = new LinearLayout(this);
        library.setOrientation(LinearLayout.VERTICAL);
        library.setPadding(0, dp(4), 0, dp(12));
        scroll.addView(library, new ScrollView.LayoutParams(-1, -2));
        menuOverlay.addView(scroll, new LinearLayout.LayoutParams(-1, 0, 1));

        for (ConsoleGroup group : groups.values()) {
            LinearLayout row = new LinearLayout(this);
            row.setOrientation(LinearLayout.VERTICAL);
            row.setPadding(dp(10), dp(7), dp(10), dp(7));
            row.setBackgroundColor(Color.rgb(31, 36, 42));

            TextView groupTitle = makeText(group.title, 16, Color.WHITE);
            row.addView(groupTitle, new LinearLayout.LayoutParams(-1, dp(30)));

            Button choose = makeMenuButton("Elegir carpeta");
            choose.setOnClickListener(view -> openFolderPicker(group));
            if (firstMenuButton == null) {
                firstMenuButton = choose;
            }
            row.addView(choose, new LinearLayout.LayoutParams(-1, dp(46)));

            TextView folderLabel = makeText("Carpeta: no seleccionada", 12, Color.LTGRAY);
            folderLabel.setPadding(0, dp(3), 0, dp(3));
            folderLabels.put(group.id, folderLabel);
            row.addView(folderLabel, new LinearLayout.LayoutParams(-1, -2));

            LinearLayout gameList = new LinearLayout(this);
            gameList.setOrientation(LinearLayout.VERTICAL);
            gameLists.put(group.id, gameList);
            Uri storedUri = readStoredFolder(group);
            if (storedUri == null) {
                gameList.addView(makeText("No hay juegos en esta carpeta", 12, Color.GRAY));
            } else {
                folderUris.put(group.id, storedUri);
                folderLabel.setText("Carpeta: " + folderName(storedUri));
                gameList.addView(makeText("Buscando juegos…", 12, Color.GRAY));
            }
            row.addView(gameList, new LinearLayout.LayoutParams(-1, -2));

            LinearLayout.LayoutParams rowParams = new LinearLayout.LayoutParams(-1, -2);
            rowParams.setMargins(0, 0, 0, dp(8));
            library.addView(row, rowParams);
        }

        mLayout.addView(menuOverlay, new ViewGroup.LayoutParams(-1, -1));
        menuOverlay.bringToFront();
        menuOverlay.post(() -> {
            if (firstMenuButton != null) {
                firstMenuButton.requestFocus();
            }
        });

        for (ConsoleGroup group : groups.values()) {
            if (folderUris.containsKey(group.id)) {
                scanGroup(group);
            }
        }
    }

    private Uri readStoredFolder(ConsoleGroup group) {
        String value = preferences.getString(FOLDER_PREFIX + group.id, null);
        if (value == null || value.isEmpty() || Build.VERSION.SDK_INT < 21) {
            return null;
        }
        try {
            return Uri.parse(value);
        } catch (RuntimeException ignored) {
            return null;
        }
    }

    private String folderName(Uri treeUri) {
        try {
            if (Build.VERSION.SDK_INT >= 21) {
                Cursor cursor = getContentResolver().query(treeUri,
                        new String[]{DocumentsContract.Document.COLUMN_DISPLAY_NAME}, null, null, null);
                if (cursor != null) {
                    try {
                        if (cursor.moveToFirst()) {
                            String name = cursor.getString(0);
                            if (name != null && !name.isEmpty()) {
                                return name;
                            }
                        }
                    } finally {
                        cursor.close();
                    }
                }
            }
        } catch (RuntimeException ignored) {
            // A few Android TV document providers reject querying a tree URI.
            // The URI itself is still usable for the recursive child query.
        }
        String segment = treeUri.getLastPathSegment();
        return segment == null ? "seleccionada" : segment;
    }

    private void openFolderPicker(ConsoleGroup group) {
        if (Build.VERSION.SDK_INT < 21) {
            Toast.makeText(this, "Este Android no permite seleccionar carpetas; elige una ROM.", Toast.LENGTH_LONG).show();
            openRomPickerLegacy(group.id);
            return;
        }
        pendingFolderGroupId = group.id;
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT_TREE);
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION
                | Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION
                | Intent.FLAG_GRANT_PREFIX_URI_PERMISSION);
        startActivityForResult(intent, OPEN_FOLDER_REQUEST);
    }

    private void openRomPickerLegacy(String groupId) {
        pendingFolderGroupId = groupId;
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("*/*");
        startActivityForResult(intent, OPEN_ROM_REQUEST);
    }

    private String displayName(Uri uri) {
        try {
            Cursor cursor = getContentResolver().query(uri, new String[]{OpenableColumns.DISPLAY_NAME}, null, null, null);
            if (cursor != null) {
                try {
                    if (cursor.moveToFirst()) {
                        return cursor.getString(0);
                    }
                } finally {
                    cursor.close();
                }
            }
        } catch (RuntimeException ignored) {
            // Fall back to the URI segment below when a provider has no metadata.
        }
        String segment = uri.getLastPathSegment();
        return segment == null ? "selected-rom" : segment;
    }

    private String safeFileName(String value) {
        String name = value == null || value.isEmpty() ? "selected-rom" : value;
        name = name.replaceAll("[^A-Za-z0-9._-]", "_");
        return name.isEmpty() ? "selected-rom" : name;
    }

    private String copyRomToInternalStorage(Uri uri, String groupId, String preferredName) throws IOException {
        File romDirectory = new File(new File(getFilesDir(), "roms"), groupId == null ? "other" : groupId);
        if (!romDirectory.exists() && !romDirectory.mkdirs()) {
            throw new IOException("Unable to create ROM directory");
        }

        String name = safeFileName(preferredName == null ? displayName(uri) : preferredName);
        File destination = new File(romDirectory, name);
        try (InputStream input = getContentResolver().openInputStream(uri);
             FileOutputStream output = new FileOutputStream(destination)) {
            if (input == null) {
                throw new IOException("Unable to open selected ROM");
            }
            byte[] buffer = new byte[64 * 1024];
            int count;
            while ((count = input.read(buffer)) != -1) {
                output.write(buffer, 0, count);
            }
        }
        return destination.getAbsolutePath();
    }

    private boolean isRomFile(String name, Set<String> extensions) {
        String lower = name.toLowerCase(Locale.US);
        for (String extension : extensions) {
            if (lower.endsWith(extension)) {
                return true;
            }
        }
        return false;
    }

    private boolean isZipFile(String name) {
        return name.toLowerCase(Locale.US).endsWith(ZIP_EXTENSION);
    }

    private boolean isAnyRomFile(String name) {
        String lower = name.toLowerCase(Locale.US);
        for (String extension : ROM_EXTENSIONS) {
            if (lower.endsWith(extension)) {
                return true;
            }
        }
        return false;
    }

    private File romDirectory(String groupId) throws IOException {
        File directory = new File(new File(getFilesDir(), "roms"), groupId == null ? "other" : groupId);
        if (!directory.exists() && !directory.mkdirs()) {
            throw new IOException("Unable to create ROM directory");
        }
        return directory;
    }

    private List<RomEntry> scanTree(Uri treeUri, Set<String> extensions) {
        if (Build.VERSION.SDK_INT < 21) {
            return Collections.emptyList();
        }
        ArrayList<RomEntry> result = new ArrayList<>();
        ArrayDeque<String> pending = new ArrayDeque<>();
        HashSet<String> visited = new HashSet<>();
        pending.add(DocumentsContract.getTreeDocumentId(treeUri));
        while (!pending.isEmpty()) {
            String parentId = pending.removeFirst();
            if (!visited.add(parentId)) {
                continue;
            }
            Uri childrenUri = DocumentsContract.buildChildDocumentsUriUsingTree(treeUri, parentId);
            Cursor cursor = getContentResolver().query(childrenUri,
                    new String[]{DocumentsContract.Document.COLUMN_DOCUMENT_ID,
                            DocumentsContract.Document.COLUMN_DISPLAY_NAME,
                            DocumentsContract.Document.COLUMN_MIME_TYPE}, null, null, null);
            if (cursor == null) {
                continue;
            }
            try {
                while (cursor.moveToNext()) {
                    String documentId = cursor.getString(0);
                    String name = cursor.getString(1);
                    String mimeType = cursor.getString(2);
                    if (DocumentsContract.Document.MIME_TYPE_DIR.equals(mimeType)) {
                        pending.addLast(documentId);
                    } else if (name != null && isRomFile(name, extensions)) {
                        result.add(new RomEntry(name,
                                DocumentsContract.buildDocumentUriUsingTree(treeUri, documentId)));
                    }
                }
            } finally {
                cursor.close();
            }
        }
        Collections.sort(result, Comparator.comparing(entry -> entry.name.toLowerCase(Locale.US)));
        return result;
    }

    private void scanGroup(ConsoleGroup group) {
        final Uri treeUri = folderUris.get(group.id);
        final LinearLayout gameList = gameLists.get(group.id);
        if (treeUri == null || gameList == null) {
            return;
        }
        gameList.removeAllViews();
        gameList.addView(makeText("Buscando juegos…", 12, Color.GRAY));
        new Thread(() -> {
            List<RomEntry> entries = null;
            Exception failure = null;
            try {
                entries = scanTree(treeUri, group.extensions);
            } catch (Exception exception) {
                failure = exception;
            }
            final List<RomEntry> found = entries;
            final Exception error = failure;
            runOnUiThread(() -> {
                gameList.removeAllViews();
                if (error != null) {
                    gameList.addView(makeText("No se pudo leer esta carpeta", 12, Color.rgb(255, 180, 160)));
                    return;
                }
                if (found == null || found.isEmpty()) {
                    gameList.addView(makeText("No hay ROMs compatibles aquí", 12, Color.GRAY));
                    return;
                }
                for (RomEntry entry : found) {
                    Button gameButton = makeMenuButton(entry.name);
                    gameButton.setTextSize(14);
                    gameButton.setOnClickListener(view -> loadRomAsync(group.id, entry));
                    LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-1, dp(44));
                    params.setMargins(dp(8), dp(2), dp(8), dp(2));
                    gameList.addView(gameButton, params);
                }
            });
        }, "mesence-scan-" + group.id).start();
    }

    private List<String> listZipRoms(Uri uri) throws IOException {
        ArrayList<String> entries = new ArrayList<>();
        try (InputStream input = getContentResolver().openInputStream(uri);
             ZipInputStream zip = input == null ? null : new ZipInputStream(input)) {
            if (zip == null) {
                throw new IOException("Unable to open ZIP archive");
            }
            ZipEntry entry;
            while ((entry = zip.getNextEntry()) != null) {
                if (!entry.isDirectory() && entry.getName() != null && isAnyRomFile(entry.getName())) {
                    entries.add(entry.getName());
                }
            }
        }
        Collections.sort(entries, String.CASE_INSENSITIVE_ORDER);
        return entries;
    }

    private String extractZipRom(Uri uri, String innerName, String groupId) throws IOException {
        String outputName = innerName;
        int slash = outputName.lastIndexOf('/');
        if (slash >= 0) {
            outputName = outputName.substring(slash + 1);
        }
        File destination = new File(romDirectory(groupId), safeFileName(outputName));
        try (InputStream input = getContentResolver().openInputStream(uri);
             ZipInputStream zip = input == null ? null : new ZipInputStream(input)) {
            if (zip == null) {
                throw new IOException("Unable to open ZIP archive");
            }
            ZipEntry entry;
            while ((entry = zip.getNextEntry()) != null) {
                if (entry.isDirectory() || !innerName.equals(entry.getName())) {
                    continue;
                }
                try (FileOutputStream output = new FileOutputStream(destination)) {
                    byte[] buffer = new byte[64 * 1024];
                    int count;
                    while ((count = zip.read(buffer)) != -1) {
                        output.write(buffer, 0, count);
                    }
                }
                return destination.getAbsolutePath();
            }
        }
        throw new IOException("ROM entry not found in ZIP archive");
    }

    private void finishRomLoad(boolean success, Exception error) {
        if (success) {
            gameRunning = true;
            menuOverlay.setVisibility(View.GONE);
            menuStatus.setText("Juego iniciado");
            setTouchControlsVisible(!hasPhysicalController());
        } else {
            menuStatus.setText("No se pudo cargar la ROM");
            Toast.makeText(this, error == null ? "MesenCE no pudo cargar esta ROM" : "No se pudo copiar la ROM",
                    Toast.LENGTH_LONG).show();
        }
    }

    private void loadZipEntryAsync(String groupId, RomEntry archive, String innerName) {
        menuStatus.setText("Cargando " + innerName + "…");
        new Thread(() -> {
            boolean loaded = false;
            Exception failure = null;
            try {
                String path = extractZipRom(archive.uri, innerName, groupId);
                loaded = nativeLoadRom(path);
            } catch (Exception exception) {
                failure = exception;
            }
            final boolean success = loaded;
            final Exception error = failure;
            runOnUiThread(() -> finishRomLoad(success, error));
        }, "mesence-load-zip-rom").start();
    }

    private void showZipEntries(String groupId, RomEntry archive, List<String> entries) {
        String[] names = entries.toArray(new String[0]);
        AlertDialog dialog = new AlertDialog.Builder(this)
                .setTitle("Elige una ROM dentro de " + archive.name)
                .setItems(names, (which, ignored) -> loadZipEntryAsync(groupId, archive, names[which]))
                .setNegativeButton("Cancelar", null)
                .create();
        dialog.setOnShowListener(ignored -> {
            if (dialog.getListView() != null && dialog.getListView().getChildCount() > 0) {
                dialog.getListView().getChildAt(0).requestFocus();
            }
        });
        dialog.show();
    }

    private void loadZipAsync(String groupId, RomEntry archive) {
        menuStatus.setText("Leyendo " + archive.name + "…");
        new Thread(() -> {
            List<String> entries = null;
            Exception failure = null;
            try {
                entries = listZipRoms(archive.uri);
            } catch (Exception exception) {
                failure = exception;
            }
            final List<String> found = entries;
            final Exception error = failure;
            runOnUiThread(() -> {
                if (error != null || found == null || found.isEmpty()) {
                    finishRomLoad(false, error == null ? new IOException("ZIP sin ROM compatible") : error);
                } else if (found.size() == 1) {
                    loadZipEntryAsync(groupId, archive, found.get(0));
                } else {
                    menuStatus.setText("Elige una ROM dentro del ZIP");
                    showZipEntries(groupId, archive, found);
                }
            });
        }, "mesence-list-zip-roms").start();
    }

    private void loadRomAsync(String groupId, RomEntry entry) {
        if (isZipFile(entry.name)) {
            loadZipAsync(groupId, entry);
            return;
        }
        menuStatus.setText("Cargando " + entry.name + "…");
        new Thread(() -> {
            boolean loaded = false;
            Exception failure = null;
            try {
                String path = copyRomToInternalStorage(entry.uri, groupId, entry.name);
                loaded = nativeLoadRom(path);
            } catch (Exception exception) {
                failure = exception;
            }
            final boolean success = loaded;
            final Exception error = failure;
            runOnUiThread(() -> finishRomLoad(success, error));
        }, "mesence-load-rom").start();
    }

    private boolean hasPhysicalController() {
        for (int id : InputDevice.getDeviceIds()) {
            InputDevice device = InputDevice.getDevice(id);
            if (device == null) {
                continue;
            }
            int sources = device.getSources();
            if ((sources & InputDevice.SOURCE_GAMEPAD) == InputDevice.SOURCE_GAMEPAD
                    || (sources & InputDevice.SOURCE_JOYSTICK) == InputDevice.SOURCE_JOYSTICK) {
                return true;
            }
        }
        return false;
    }

    private int mapGameKey(int keyCode) {
        switch (keyCode) {
            case KeyEvent.KEYCODE_DPAD_UP:
                return KEY_UP;
            case KeyEvent.KEYCODE_DPAD_DOWN:
                return KEY_DOWN;
            case KeyEvent.KEYCODE_DPAD_LEFT:
                return KEY_LEFT;
            case KeyEvent.KEYCODE_DPAD_RIGHT:
                return KEY_RIGHT;
            case KeyEvent.KEYCODE_BUTTON_A:
                return KEY_A;
            case KeyEvent.KEYCODE_BUTTON_B:
                return KEY_B;
            case KeyEvent.KEYCODE_BUTTON_X:
                return KEY_X;
            case KeyEvent.KEYCODE_BUTTON_Y:
                return KEY_Y;
            case KeyEvent.KEYCODE_BUTTON_L1:
                return KEY_L;
            case KeyEvent.KEYCODE_BUTTON_R1:
                return KEY_R;
            case KeyEvent.KEYCODE_BUTTON_SELECT:
                return KEY_SELECT;
            case KeyEvent.KEYCODE_BUTTON_START:
                return KEY_START;
            default:
                return -1;
        }
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        if (gameRunning) {
            int mesenKey = mapGameKey(event.getKeyCode());
            if (mesenKey >= 0) {
                if (event.getAction() == KeyEvent.ACTION_DOWN) {
                    nativeSetKeyState(mesenKey, true);
                } else if (event.getAction() == KeyEvent.ACTION_UP) {
                    nativeSetKeyState(mesenKey, false);
                }
                return true;
            }
        }
        return super.dispatchKeyEvent(event);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        preferences = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
        initConsoleGroups();
        if (inputManager == null) {
            inputManager = (InputManager) getSystemService(INPUT_SERVICE);
            if (inputManager != null) {
                inputManager.registerInputDeviceListener(this, null);
            }
        }
        if (!mBrokenLibraries && mLayout != null) {
            addTouchControls();
            buildLibraryMenu();
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (resultCode != RESULT_OK || data == null || data.getData() == null) {
            return;
        }
        Uri uri = data.getData();
        if (requestCode == OPEN_FOLDER_REQUEST && pendingFolderGroupId != null && Build.VERSION.SDK_INT >= 21) {
            try {
                int takeFlags = data.getFlags() & (Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
                if (takeFlags != 0) {
                    getContentResolver().takePersistableUriPermission(uri, takeFlags);
                }
            } catch (RuntimeException ignored) {
                // Some TV document providers do not offer persistable permissions.
            }
            String groupId = pendingFolderGroupId;
            pendingFolderGroupId = null;
            try {
                folderUris.put(groupId, uri);
                preferences.edit().putString(FOLDER_PREFIX + groupId, uri.toString()).apply();
                TextView label = folderLabels.get(groupId);
                if (label != null) {
                    label.setText("Carpeta: " + folderName(uri));
                }
                ConsoleGroup group = groups.get(groupId);
                if (group != null) {
                    scanGroup(group);
                }
            } catch (RuntimeException exception) {
                folderUris.remove(groupId);
                Toast.makeText(this, "No se pudo leer esa carpeta en este Android TV", Toast.LENGTH_LONG).show();
            }
        } else if (requestCode == OPEN_ROM_REQUEST) {
            String groupId = pendingFolderGroupId == null ? "other" : pendingFolderGroupId;
            pendingFolderGroupId = null;
            loadRomAsync(groupId, new RomEntry(displayName(uri), uri));
        }
    }

    @Override
    public void onInputDeviceAdded(int deviceId) {
        if (gameRunning) {
            setTouchControlsVisible(!hasPhysicalController());
        }
    }

    @Override
    public void onInputDeviceRemoved(int deviceId) {
        if (gameRunning) {
            setTouchControlsVisible(!hasPhysicalController());
        }
    }

    @Override
    public void onInputDeviceChanged(int deviceId) {
        if (gameRunning) {
            setTouchControlsVisible(!hasPhysicalController());
        }
    }

    @Override
    public void onBackPressed() {
        if (gameRunning) {
            gameRunning = false;
            setTouchControlsVisible(false);
            if (menuOverlay != null) {
                menuOverlay.setVisibility(View.VISIBLE);
                menuOverlay.bringToFront();
                menuOverlay.post(() -> {
                    if (firstMenuButton != null) {
                        firstMenuButton.requestFocus();
                    }
                });
            }
            return;
        }
        super.onBackPressed();
    }

    @Override
    protected void onPause() {
        // Do not leave a virtual button held when Android pauses the activity.
        if (!mBrokenLibraries) {
            nativeSetKeyState(KEY_UP, false);
            nativeSetKeyState(KEY_DOWN, false);
            nativeSetKeyState(KEY_LEFT, false);
            nativeSetKeyState(KEY_RIGHT, false);
            nativeSetKeyState(KEY_A, false);
            nativeSetKeyState(KEY_B, false);
            nativeSetKeyState(KEY_X, false);
            nativeSetKeyState(KEY_Y, false);
            nativeSetKeyState(KEY_L, false);
            nativeSetKeyState(KEY_R, false);
            nativeSetKeyState(KEY_SELECT, false);
            nativeSetKeyState(KEY_START, false);
        }
        super.onPause();
    }

    @Override
    protected void onDestroy() {
        if (inputManager != null) {
            inputManager.unregisterInputDeviceListener(this);
        }
        super.onDestroy();
    }

    @Override
    protected String[] getArguments() {
        return new String[0];
    }

    private static native boolean nativeLoadRom(String path);
    private static native void nativeSetKeyState(int keyCode, boolean pressed);
}
