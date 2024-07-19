# Hands-on, Hands-off Interaction
Open sourcing of the Hands-on, Hands-off Interaction presented in DOI:10.1145/3654777.3676331

This repository contains the code for adding bimanual gaze-assisted interaction to Meta XR SDK.
The Unity project code contains an example scene with the interaction combinations already set up.

Before importing the GazeBimanualMetaSDK-v1.0 package, you will need the following dependencies from the Asset Store:
 - Meta XR All-in-One SDK (v62+)
 - Quick Outline (v1.1) (note that, overlapping outlines may not be visible)

If one wishes to add the interaction combinations to their project, simply import the "GazeBimanual-MetaSDK.unitypackage" in Unity 2022, then add the prefab "Gaze Bimanual Interaction" in "Assets\GazeBimanual\Prefabs" to the scene. 
After doing so, add an "IndirectGazeInteractable" component to a game object to enable indirect interaction (with Meta XR SDK "HandGrabInteractable" to enable direct interaction).
Then, switching between direct and indirect interaction is seamless and fully hand-agnostic.
