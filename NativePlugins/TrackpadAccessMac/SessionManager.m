//
//  CustomVC.h
//  TrackpadTouch
//
//  Created by Omar Calero on 6/27/18.
//  Copyright © 2018 Omar Calero. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "SessionManager.h"

@implementation SessionManager
@synthesize vc, mainWindowInstance;

+ (SessionManager*)sharedInstance{
    static dispatch_once_t pred;
    static SessionManager *shared = nil;
    dispatch_once(&pred, ^{
        shared = [[SessionManager alloc] init];
        [shared performSelector:@selector(startWindow) withObject:nil afterDelay:0.3];
    });
    return shared;
}

- (void)startWindow{
    self.mainWindowInstance = [NSApp keyWindow];
    self.vc = [[CustomVC alloc] initWithViewFrame:NSMakeRect(0, 0, self.mainWindowInstance.frame.size.width, self.mainWindowInstance.frame.size.height)];
    [self.mainWindowInstance.contentView addSubview:vc.view];
}
@end
