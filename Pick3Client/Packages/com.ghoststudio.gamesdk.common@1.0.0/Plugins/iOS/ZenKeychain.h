#import <Foundation/Foundation.h>

@interface ZenKeychain : NSObject
+ (void)save:(NSString *)service data:(id)data;
+ (id)load:(NSString *)service;
+ (void)remove:(NSString *)service;
@end
