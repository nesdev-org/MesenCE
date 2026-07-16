package ca.mesen.android;

import android.os.Bundle;

import org.libsdl.app.SDLActivity;

/**
 * SDL's Android activity owns the surface and dispatches lifecycle/input
 * events to the native Mesen entry point.  The emulator core is kept native
 * so the same C++ sources are used by desktop and Android builds.
 */
public final class MesenActivity extends SDLActivity {
    @Override
    protected String[] getArguments() {
        // ROM file selection and touch controls will be added in the next
        // Android iteration.  Keeping this empty starts the native shell
        // without making assumptions about storage permissions.
        return new String[0];
    }
}
