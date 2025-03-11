#!/bin/bash

# Purpose: The bash script is designed to automate the process of running multiple Android Package (APK) 
# files on connected Android devices and monitor the device's temperature. 
# If the temperature exceeds a specified threshold, the script will pause the APK execution until the device cooldown. 
#
# Usage: ./auto-capture apk1.apk apk2.apk 
# --
#
# --
# Last updated: 19/Sep/2024
# --------------------------------------------------------------------------------

# Set variables

# Temperature
target_temp=30 # in celcius

# Capture
enable_fixed_performance_mode=false
capture_time=1000 # Capture duration

# APK parameters
package_name="com.unity.megacity.metro"
storage_path="/storage/emulated/0/Android/data/com.unity.megacity.metro/files/"

# Test results
result_folder="results"

# --------------------------------------------------------------------------------
# Functions
# --------------------------------------------------------------------------------

# Cooldown Phone to target_temp
cooldown_device () {
    
    # Poll battery temperature
    INPUT="$(adb shell dumpsys battery)"
    let target=target_temp*10
    temperature=$(sed -nr '/temperature:/ s/.*temperature: ([^\s]+).*/\1/p' <<<"$INPUT")
    let display_decimal=temperature%10
    let display_temperature=temperature/10
    echo "Current temperature ${display_temperature}.${display_decimal}c at `date`"
    
    while [ $target -lt $temperature ]; do
        echo "sleep for $cooldown_time seconds, current temperature is ${display_temperature}.${display_decimal}c, target temperature is ${target_temp}c"
        sleep $cooldown_time
        INPUT="$(adb shell dumpsys battery)"
        temperature=$(sed -nr '/temperature:/ s/.*temperature: ([^\s]+).*/\1/p' <<<"$INPUT")
        let display_decimal=temperature%10
        let display_temperature=temperature/10
    done
}

# --------------------------------------------------------------------------------
# Main program
# --------------------------------------------------------------------------------
cooldown_time=10 # interval time in seconds when polling temperature
let test_timeout=capture_time+10
phonemodel="$(adb shell getprop ro.product.model)"
echo "Phone model: ${phonemodel}"

declare -a apklist="($@)"

mkdir $result_folder

for apk in "${apklist[@]}";    
do 
    if [ ! -z "$apk" -a "$apk" != " " ];  then
        echo "APK: $apk"
        adb shell cmd power set-fixed-performance-mode-enabled $enable_fixed_performance_mode
        apkname=$(basename -- ${apk%.*})
        echo $apkname

        #install
        adb install $apk        

        cooldown_device       

        #Start app
        echo "Start Time: " `date`
        adb shell monkey -p $package_name 1        


        picDir=${phonemodel}_${apkname}

        # Wait until time is up
        COUNTER=0
        while [  $COUNTER -lt $test_timeout ]; do
            
            let COUNTER=COUNTER+1
            if [ $capture_time -lt $COUNTER ]; then
                
                echo "Copy files to computer";
                cd $result_folder
                mkdir "$picDir"
                cd "$picDir"
                
                adb shell ls $storage_path*.png | tr -d '\r' | sed -e 's/^\///' | xargs -n1 adb pull
                adb shell ls $storage_path*.csv | tr -d '\r' | sed -e 's/^\///' | xargs -n1 adb pull
                for file in *.png ; do mv $file "${file//changeme/$picDir}" ; done
                for file in *.csv ; do mv $file "${file//changeme/$picDir}" ; done
                cd ..
                cd ..
        
                break
            else
                echo The time is $COUNTER / $capture_time
                sleep 1
            fi
        done

        echo "End Time: " `date`

        #quit        
        adb shell input keyevent 3 #home key
        adb uninstall $package_name
	
        adb shell cmd power set-fixed-performance-mode-enabled false
    fi    
done

adb shell cmd power set-fixed-performance-mode-enabled false