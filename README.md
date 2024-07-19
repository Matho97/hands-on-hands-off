[![Generic badge](https://img.shields.io/badge/Maintained-Yes-green.svg)](https://shields.io/)
[![Generic badge](https://img.shields.io/badge/Software-C%23-blue.svg)](https://shields.io/)
[![Generic badge](https://img.shields.io/badge/License-MIT-red.svg)](https://shields.io/)
[![DOI](https://img.shields.io/badge/DOI-10.1145%2F3379156.3391374-yellowgreen)](https://doi.org/10.1145/3654777.3676331)

# Hands-on, Hands-off Interaction
Open sourcing of the Hands-on, Hands-off Interaction published at the UIST 2024 conference.

This repository contains the code for adding bimanual gaze-assisted interaction to Meta XR SDK.
The Unity project code contains an example scene with the interaction combinations already set up.

Before importing the GazeBimanualMetaSDK-v1.0 package, you will need the following dependencies from the Asset Store:
 - Meta XR All-in-One SDK (v62+)
 - Quick Outline (v1.1) (note that, overlapping outlines may not be visible)

If one wishes to add the interaction combinations to their project, simply import the "GazeBimanual-MetaSDK.unitypackage" in Unity 2022, then add the prefab "Gaze Bimanual Interaction" in "Assets\GazeBimanual\Prefabs" to the scene. 
After doing so, add an "IndirectGazeInteractable" component to a game object to enable indirect interaction (with Meta XR SDK "HandGrabInteractable" to enable direct interaction).
Then, switching between direct and indirect interaction is seamless and fully hand-agnostic.
