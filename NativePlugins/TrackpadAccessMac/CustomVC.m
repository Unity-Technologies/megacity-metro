//
//  CustomVC.m
//  TrackpadTouch
//
//  Created by Omar Calero on 6/27/18.
//  Copyright © 2018 Omar Calero. All rights reserved.
//

#import "CustomVC.h"
#import "SessionManager.h"

@interface CustomVC ()

@end

@implementation CustomVC

- (id)initWithViewFrame:(NSRect)_rect{
    self = [super init];
    if (self){
        NSView *theView = [[NSView alloc] initWithFrame:_rect];
        [self.view setWantsLayer:YES];
        self.view.layer.backgroundColor = [[NSColor redColor] CGColor];
        self.view = theView;
    }
    return self;
}

- (void)loadView{
    
}

- (void)viewWillAppear {
    [super viewDidLoad];
    
    [self.view setAllowedTouchTypes:NSTouchTypeMaskIndirect];
    [self.view setWantsRestingTouches:YES];
}

- (void)setRepresentedObject:(id)representedObject {
    [super setRepresentedObject:representedObject];
}

- (void)touchesBeganWithEvent:(NSEvent *)event{
    NSSet<NSTouch*> *touches = [event allTouches];
    NSArray *touchesArray = [touches allObjects];
    SessionManager *sessionManager = [SessionManager sharedInstance];
    sessionManager.touchesCount = (int)[touchesArray count];
}

- (void)touchesMovedWithEvent:(NSEvent *)event{
    NSSet<NSTouch*> *touches = [event allTouches];    
    NSArray *touchesArray = [touches allObjects];
    SessionManager *sessionManager = [SessionManager sharedInstance];
    sessionManager.touchesCount = (int)[touchesArray count];
}

- (void)touchesEndedWithEvent:(NSEvent *)event{
    NSSet<NSTouch*> *touches = [event allTouches];
    NSArray *touchesArray = [touches allObjects];
    
    BOOL allTouchesEnded = YES;
    for (NSTouch *touch in touchesArray) {
        if (touch.phase != NSTouchPhaseEnded && 
            touch.phase != NSTouchPhaseCancelled) {
            allTouchesEnded = NO;
            break;
        }
    }
    SessionManager *sessionManager = [SessionManager sharedInstance];
    sessionManager.touchesCount = allTouchesEnded ? 0 : (int)[touchesArray count];
}
@end
