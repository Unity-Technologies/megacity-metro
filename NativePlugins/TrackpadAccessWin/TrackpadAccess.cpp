// TrackpadAccess.cpp : Defines the exported functions for the DLL.
//
#include "pch.h"
#include "framework.h"
#include "TrackpadAccess.h"
#include "Windows.h"

// Global Variables, win and initialization state
HHOOK g_hMouseHook = NULL; // Global mouse hook handle
int g_touchCount = 0; // Global variable to store touch count

// Mouse hook procedure
LRESULT CALLBACK MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
    if (nCode >= 0) 
    {
        // Check if it's a mouse event we want to count as touch input
        if (wParam == WM_LBUTTONDOWN || wParam == WM_MOUSEMOVE) 
        {
            // Increment touch count
            g_touchCount++;
        }
    }

    // Call the next hook in the hook chain
    return CallNextHookEx(g_hMouseHook, nCode, wParam, lParam);
}

extern "C" 
{
    TRACKPADACCESS_API void InitTrackpadCommunication() 
    {
        g_hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProc, NULL, 0);
    }

    TRACKPADACCESS_API void CloseTrackpadCommunication()
    {
        if (g_hMouseHook != NULL) {
            UnhookWindowsHookEx(g_hMouseHook);
            g_hMouseHook = NULL;
        }
    }

    TRACKPADACCESS_API int TrackpadTouchesCount() 
    {
        int tempTouches = g_touchCount;
        g_touchCount = 0;
        return tempTouches;
    }
}
