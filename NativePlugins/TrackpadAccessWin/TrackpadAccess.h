#pragma once

#ifdef TRACKPADACCESS_EXPORTS
#define TRACKPADACCESS_API __declspec(dllexport)
#else
#define TRACKPADACCESS_API __declspec(dllimport)
#endif

#include <Windows.h>

// Mouse hook procedure
LRESULT CALLBACK MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam);

extern "C"
{
    // Initialize trackpad communication
    TRACKPADACCESS_API void InitTrackpadCommunication();

    // Close trackpad communication
    TRACKPADACCESS_API void CloseTrackpadCommunication();

    // Get the count of trackpad touches
    TRACKPADACCESS_API int TrackpadTouchesCount();
}
