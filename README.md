## Fork of mcaddy/audipoi

This is my fork of mcaddy/audipoi utility.

Currently that seems to be broken because it is uploading your PGPSW zip files to an Azure cloud service, which is now offline or unavailable.

This version was forked from the original 0.9 version and doesn't include that upload.  It may be missing other fixes and features also - Sorry.

I've also modified it to produce more attractive icons with speed limits.
Unfortunately this currently results in too many categories being generated, as my MIB2 system has a maximum of 10 arrival alerts that can be set.
Not an issue for me as I never have alerts set for Mobile cameras, and you can have as many categories displayed as you want on map contents.

On my To Do list is to provide separate combine categories for Fixed, Mobile, Specs and RedLight (to be used for alerts) as well as the new categories (to be used for map icons).
I think that should work.

## SpeedCameraToPoi

This application downloads the latest Camera Database from PocketGPSWorld and builds a POI database, if present it will retain the database already on the target drive, replacing the Camera categories if present.

Currently the app will group cameras into categories, separated by speed:-

* Fixed - Gatso, Truvelo, Monitorn, RedSpeed
* Mobile - Verified Mobile camera (safty camera partnersip) sites
* Specs - Average Speed cameras
* RedLight - Traffic Light cameras (some of which double up as speed cameras)

Latest version - [v1.0.1.0](https://github.com/pcbbc/audipoi/raw/master/Releases/SpeedCameraToPoi_v1.0.1.0.zip)

To install unpack the contents of the zip file into a folder of your choice and run SpeedCameraToPoi.exe

## GeocacheToPoi

This application takes a GPX file of Geocaches and builds a POI database, if present it will retain the database already on the target drive, replacing the Geocache categories if present.

Currently the app will group caches by type, the following types will be created and feature in the MyAudi Special Destinations

* Caches - Earthcaches
* Caches - Letterbox
* Caches - Multi
* Caches - Traditional
* Caches - Mystery
* Caches - Virtual
* Caches - Wherigo
* Caches - Webcam

Optionally users can decide to exclude their own caches or found caches.

Latest version - [v1.0.1.0](https://github.com/pcbbc/audipoi/raw/master/Releases/GeocacheToPoi_v1.0.1.0.zip)

if you have features you'd like to see send me a mail
