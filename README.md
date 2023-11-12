# com.nobunatelier.ink
NobunAtelier extension for Ink integration.

## Getting Startedd
1. Import your assets
2. Prepare your data collections
    - StoryActorCollection
    - StorySceneCollection
    - StoryAudioCollection
3. Design your UI
    - You can use Visual Novel template. It is using VisualNovelManager with VisualNovelStoryModule
    - If making a custom dialogue that is needs custom rules, you will need to create a custom StoryModule to bind the command
4. Add to the scene StoryManager and required StoryModule (Audio, Screen Effect,  Visual Novel or custom...)
5. Write your story and make use of the provided commands (system, screen, audio, actor, ...)

## North Star
### Resource Handling
The package is built using Addressable, all resources handled by the managers are loaded and unloaded when required (audio, sprites).

### Collection Handling
Originally, there was only one collection per module (actor, scene, audio) and everything registered one time.
This is not a problem as the resources are only loaded on demand.
But I wonmder if at some point there would be a need for writter to specify a collection to use... like at the beginning of a node...