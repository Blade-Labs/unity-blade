# [WIP] Blade Unity SDK 

## Install

1. Goto menu Window > Package Manager 
2. Press Add button
3. Add package from git URL...
4. Put this repo url: `git@github.com:Blade-Labs/unity-blade.git`

## Examples

1. In Package Manager pick `Unity Blade SDK`
2. Open package Samples tab
3. Click import at `Example Blade usage` item
4. On Project navigator go to: Assets/Samples/Unity Blade SDK/0.0.1/Example Blade usage/Scenes
5. Open `RunBladeExample` scene
6. Play

## Tests

Code covered with tests. To run it do this steps:

1. Enable tests for a package: https://docs.unity3d.com/Manual/cus-tests.html#tests
    - Open project manifest.json file
    - Add field testables. Should be similar to this: `"testables": ["io.bladelabs.unity-sdk"]`
2. In Unity go to Window -> General -> Test runner
3. In "PlayMode" tab press "RunAll"

## Development

### Plugin

### JS

on `https://github.com/Blade-Labs/blade-sdk.js` call `npm run publish:unity`

### Backend for executing transactions 
