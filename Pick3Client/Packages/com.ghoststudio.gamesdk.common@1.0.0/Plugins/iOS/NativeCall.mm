#import <Foundation/Foundation.h>
#import <UIKit/UIFeedbackGenerator.h>
#import "ZenKeychain.h"
#import <sys/param.h>
#import <sys/mount.h>
#import "Classes/Unity/UnityInterface.h"

extern "C" {
    void _SaveToKeychain(const char *service, const char *data) {
        if (!service || !data) return;
        NSString *serviceString = [NSString stringWithUTF8String:service];
        NSString *dataString = [NSString stringWithUTF8String:data];
        [ZenKeychain save:serviceString data:dataString];
    }
    
    const char* _LoadFromKeychain(const char *service) {
        if (!service) return NULL;
        NSString *serviceString = [NSString stringWithUTF8String:service];
        id data = [ZenKeychain load:serviceString];
        if (data) {
            return strdup([data UTF8String]);
        }
        return NULL;
    }
    
    void _RemoveFromKeychain(const char *service) {
        if (!service) return;
        NSString *serviceString = [NSString stringWithUTF8String:service];
        [ZenKeychain remove:serviceString];
    }

    void _SetVibratoriOS() {
        UIImpactFeedbackGenerator *feedBackGenertor = [[UIImpactFeedbackGenerator alloc]initWithStyle:UIImpactFeedbackStyleHeavy];
        [feedBackGenertor prepare];
        [feedBackGenertor impactOccurred];
    }

    long _GetFreeSpace()  {
        uint64_t totalSpace = 0;
        uint64_t totalFreeSpace = 0;
        NSError *error = nil;
        NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
        NSDictionary *dictionary = [[NSFileManager defaultManager] attributesOfFileSystemForPath:[paths lastObject] error: &error];
        long long s = 0;
        if (dictionary.count) {
            NSNumber *fileSystemSizeInBytes = dictionary[NSFileSystemSize];
            NSNumber *freeFileSystemSizeInBytes = dictionary[NSFileSystemFreeSize];
            totalSpace = [fileSystemSizeInBytes unsignedLongLongValue];
            totalFreeSpace = [freeFileSystemSizeInBytes unsignedLongLongValue];
            s = totalFreeSpace/ (1024.0 * 1024.0);
        }
        return s;
    }
    
    const char* _GetiOSVersionCode() {
         NSString *build = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleVersion"];
         return strdup([build UTF8String]);
    }
}
