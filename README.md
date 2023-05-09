# Inochi2D for Unity

This is a very early work-in-progress Unity package to read and use Inochi2D models.

For the time being this implementation will be pure C#, eventually it may be rewritten to use inochi2d-c as a backend to handle the non-rendering backend of puppets.


## How to Use

Currently there's a lot of manual work involved with rendering a model, I plan to simplify usability once the core functionality works, but here's how to set up Inochi2D in a scene currently.

1. Add a `Scene` component to a game object in your Unity Scene, assign the render target textures for the `Scene` component.
2. Load a model either by dragging an INP or INX file in to your project, or use PuppetLoader to load models at runtime.
3. Add the model in to your scene, and set the Scene property to your scene
4. Attach the scene output render textures to a Material on a MeshRenderer with a MeshFilter with a Quad mesh, resize to viewport size.

## Why is the `Scene` object needed?

Inochi2D's rendering pipeline is fundamentally incompatible with Unity, as such we use CommandBuffer to internally construct our own rendering pipeline. This custom pipeline renders to a set of textures that *are* compatible with Unity rendering pipelines.

The Inochi2D output framebuffer provides: Albedo, Emission and Normal/Bumpmapping.