# Sound Controller for Unity3D

This is a fork of [Dimixar's Sound Controller](https://github.com/dimixar/audio-controller-unity) which I tailored to my needs. Kudos for Dimixar for creating the original!

Main changes:
- Sound items are referenced by id instead of name. This is less flexible, but makes it much easier to find and to rename items. This change is not backward compatible with existing SoundControllerData instances created with the Dimixar's OSSC!
- Removed tag support.
- Added priority setting to categories.
- Moved mixer setting to categories.
- Added play mode setting to sound items. Features:
  - Sequence: plays sound items sequentially.
  - Random: plays a random sound item.
  - Loop One Clip: loops one sound item.
  - Loop Sequence: loops a list of sound items.
  - Intro Loop Outro Sequence: plays an intro clip, then loops the second clip until it's stopped with a StopSequence call. Plays the outro clip at this point.
- Added setting for the minimum time between playing the same sound item again.
- Override category priority at sound item level.
- Preview sound item clips.
- Convinience features in the sound controller inspector like navigation, duplicating items, saving state to remember selected category and sound item.
- Changed sound controller inspector layout.
- Singleton sound controller - for now. This might very well change.
- Stack based object pool.
- Overridable position in PlaySoundSettings when parenting to a GameObject.
- Coroutine manegement with [MEC](http://trinary.tech/category/mec/)

![alt text][screen-mod]

## How to add it to your project

Unitypackage not included.
1. Add [MEC](https://assetstore.unity.com/packages/tools/animation/more-effective-coroutines-free-54975) to your project if you haven't already.
2. Copy the contents of the Assets folder under your Assets folder the way you like to organize your external assets.
3. Copy Editor Default Resources under your Assets folder.

[screen-mod]: https://github.com/entim/audio-controller-unity/blob/develop/screenshot.PNG

## Original Readme

This is a sound controller. The main idea of this plugin is to not use raw AudioSource, but have a nice wrapper around it from which you can easily control what sounds at what time you want to play.
The reason I made this plugin is that I don't really want to pay money for Audiotoolkit from unity asset store, and I didn't really liked it when I had the chance to use it.

![alt text][screen]

[screen]: https://github.com/dimixar/audio-controller-unity/blob/master/screenshot.PNG

Main Features:
- Organize sounds by categories
- Add tags to your sounds to filter them even further
- Add multiple variants of the same sound and easily play at random.
- Change Audioclips, edit category names, and sound names from play mode without the fear of losing those changes
- All data is saved on a scriptable object asset
- You can create multiple data assets and swap them easily
- Create whole cues of sounds with ease (very useful when you have dialog Voice Over)
- Add Random Pitch to sound items
- Add Random Volume to sound items
- It's not a singleton!!!

How to add it to your project and scene:
1. Extract the latest .unitypackage file found in releases (link here: https://github.com/dimixar/audio-controller-unity/releases);
2. Create new SoundControllerData asset using the contextual menu from Project hierarchy;
3. Find the prefab with SoundController in OSSC folder and drag it onto the scene;
4. Add your new created asset into the corresponding object field of the SoundContoller component;
5. Select gameobject with SoundController and start adding categories and items.

How to use it:
- Check the example Scene for that.
