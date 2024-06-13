//
//  CustomVC.h
//  TrackpadTouch
//
//  Created by Omar Calero on 6/27/18.
//  Copyright © 2018 Omar Calero. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SessionManager.h"

extern "C" int TrackpadTouchesCount()
{
    SessionManager *sessionManager = [SessionManager sharedInstance];
    return sessionManager.touchesCount;
}
