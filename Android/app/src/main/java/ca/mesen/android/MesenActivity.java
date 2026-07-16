package ca.mesen.android;

import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.provider.OpenableColumns;
import android.view.Gravity;
import android.view.MotionEvent;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import android.widget.Space;
import android.widget.Toast;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;

import org.libsdl.app.SDLActivity;

/**
 * SDL's Android activity owns the surface and dispatches lifecycle/input
 * events to the native Mesen entry point.  The emulator core is kept native
 * so the same C++ sources are used by desktop and Android builds.
 */
public final class MesenActivity extends SDLActivity {
    private static final int OPEN_ROM_REQUEST = 1001;

    // Values match Core/Shared/KeyDefinitions.h.
    private static final int KEY_UP = 24;
    private static final int KEY_DOWN = 26;
    private static final int KEY_LEFT = 23;
    private static final int KEY_RIGHT = 25;
    private static final int KEY_SELECT = 48; // E
    private static final int KEY_START = 61;  // R
    private static final int KEY_A = 67;      // X
    private static final int KEY_B = 69;      // Z

    private int dp(float value) {
        return (int) (value * getResources().getDisplayMetrics().density + 0.5f);
    }

    private Button makeButton(String label, int keyCode) {
        Button button = new Button(this);
        button.setText(label);
        button.setTextSize(12);
        button.setAllCaps(false);
        button.setContentDescription(label);
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

    private LinearLayout makeDPad() {
        LinearLayout dpad = new LinearLayout(this);
        dpad.setOrientation(LinearLayout.VERTICAL);
        dpad.setGravity(Gravity.CENTER);

        LinearLayout upRow = new LinearLayout(this);
        upRow.setGravity(Gravity.CENTER);
        upRow.addView(makeButton("▲", KEY_UP), new LinearLayout.LayoutParams(dp(52), dp(52)));
        dpad.addView(upRow, new LinearLayout.LayoutParams(-2, dp(52)));

        LinearLayout middleRow = new LinearLayout(this);
        middleRow.setGravity(Gravity.CENTER);
        middleRow.addView(makeButton("◀", KEY_LEFT), new LinearLayout.LayoutParams(dp(52), dp(52)));
        Space center = new Space(this);
        middleRow.addView(center, new LinearLayout.LayoutParams(dp(52), dp(52)));
        middleRow.addView(makeButton("▶", KEY_RIGHT), new LinearLayout.LayoutParams(dp(52), dp(52)));
        dpad.addView(middleRow, new LinearLayout.LayoutParams(-2, dp(52)));

        LinearLayout downRow = new LinearLayout(this);
        downRow.setGravity(Gravity.CENTER);
        downRow.addView(makeButton("▼", KEY_DOWN), new LinearLayout.LayoutParams(dp(52), dp(52)));
        dpad.addView(downRow, new LinearLayout.LayoutParams(-2, dp(52)));
        return dpad;
    }

    private LinearLayout makeActionPad() {
        LinearLayout actions = new LinearLayout(this);
        actions.setOrientation(LinearLayout.VERTICAL);
        actions.setGravity(Gravity.CENTER);

        LinearLayout face = new LinearLayout(this);
        face.setGravity(Gravity.CENTER);
        face.addView(makeButton("B", KEY_B), new LinearLayout.LayoutParams(dp(58), dp(58)));
        face.addView(makeButton("A", KEY_A), new LinearLayout.LayoutParams(dp(58), dp(58)));
        actions.addView(face, new LinearLayout.LayoutParams(-2, dp(58)));

        LinearLayout system = new LinearLayout(this);
        system.setGravity(Gravity.CENTER);
        system.addView(makeButton("Select", KEY_SELECT), new LinearLayout.LayoutParams(dp(78), dp(44)));
        system.addView(makeButton("Start", KEY_START), new LinearLayout.LayoutParams(dp(78), dp(44)));
        actions.addView(system, new LinearLayout.LayoutParams(-2, dp(44)));
        return actions;
    }

    private void addTouchControls() {
        RelativeLayout.LayoutParams dpadParams = new RelativeLayout.LayoutParams(-2, -2);
        dpadParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
        dpadParams.addRule(RelativeLayout.ALIGN_PARENT_START);
        dpadParams.setMargins(dp(10), 0, 0, dp(10));
        mLayout.addView(makeDPad(), dpadParams);

        RelativeLayout.LayoutParams actionParams = new RelativeLayout.LayoutParams(-2, -2);
        actionParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
        actionParams.addRule(RelativeLayout.ALIGN_PARENT_END);
        actionParams.setMargins(0, 0, dp(10), dp(10));
        mLayout.addView(makeActionPad(), actionParams);
    }

    private void openRomPicker() {
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("*/*");
        startActivityForResult(intent, OPEN_ROM_REQUEST);
    }

    private String displayName(Uri uri) {
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
        String segment = uri.getLastPathSegment();
        return segment == null ? "selected-rom" : segment;
    }

    private String copyRomToInternalStorage(Uri uri) throws IOException {
        File romDirectory = new File(getFilesDir(), "roms");
        if (!romDirectory.exists() && !romDirectory.mkdirs()) {
            throw new IOException("Unable to create ROM directory");
        }

        String displayName = displayName(uri);
        if (displayName == null || displayName.isEmpty()) {
            displayName = "selected-rom";
        }
        String name = displayName.replaceAll("[^A-Za-z0-9._-]", "_");
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

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (!mBrokenLibraries) {
            Button openRom = new Button(this);
            openRom.setText("Open ROM");
            openRom.setOnClickListener(view -> openRomPicker());
            RelativeLayout.LayoutParams openParams = new RelativeLayout.LayoutParams(-2, dp(48));
            openParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
            openParams.addRule(RelativeLayout.ALIGN_PARENT_START);
            openParams.setMargins(dp(8), dp(8), 0, 0);
            mLayout.addView(openRom, openParams);
            addTouchControls();
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode != OPEN_ROM_REQUEST || resultCode != RESULT_OK || data == null || data.getData() == null) {
            return;
        }
        try {
            String path = copyRomToInternalStorage(data.getData());
            if (!nativeLoadRom(path)) {
                Toast.makeText(this, "MesenCE could not load this ROM", Toast.LENGTH_LONG).show();
            }
        } catch (IOException exception) {
            Toast.makeText(this, "Could not copy the selected ROM", Toast.LENGTH_LONG).show();
        }
    }

    @Override
    protected void onPause() {
        // Do not leave a virtual button held when Android pauses the activity
        // (for example when the document picker opens).
        nativeSetKeyState(KEY_UP, false);
        nativeSetKeyState(KEY_DOWN, false);
        nativeSetKeyState(KEY_LEFT, false);
        nativeSetKeyState(KEY_RIGHT, false);
        nativeSetKeyState(KEY_A, false);
        nativeSetKeyState(KEY_B, false);
        nativeSetKeyState(KEY_SELECT, false);
        nativeSetKeyState(KEY_START, false);
        super.onPause();
    }

    @Override
    protected String[] getArguments() {
        return new String[0];
    }

    private static native boolean nativeLoadRom(String path);
    private static native void nativeSetKeyState(int keyCode, boolean pressed);
}
