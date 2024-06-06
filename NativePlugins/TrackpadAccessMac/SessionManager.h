//
//  CustomVC.h
//  TrackpadTouch
//
//  Created by Omar Calero on 6/27/18.
//  Copyright © 2018 Omar Calero. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>
#import "CustomVC.h"

@interface SessionManager : NSObject{
@public
    
}

@property int touchesCount;
@property(nonatomic, strong)CustomVC *vc;
@property(nonatomic, strong)NSWindow *mainWindowInstance;

+(SessionManager*)sharedInstance;

@end
