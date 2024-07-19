[![Generic badge](https://img.shields.io/badge/Maintained-Yes-green.svg)](https://shields.io/)
[![Generic badge](https://img.shields.io/badge/Software-C%23-blue.svg)](https://shields.io/)
[![Generic badge](https://img.shields.io/badge/License-MIT-red.svg)](https://shields.io/)
[![DOI](https://img.shields.io/badge/DOI-10.1145%2F3379156.3391374-yellowgreen)](https://doi.org/10.1145/3654777.3676331)

# Hands-on, Hands-off Interaction publication
Open sourcing of the Hands-on, Hands-off Interaction published at the UIST 2024 conference.

See our paper for more details:

```bibtex
@inproceedings{Lystbaek24HandsOnHandsOff,
    title        = {Hands-on, Hands-off: Gaze-Assisted Bimanual 3D Interaction},
    author       = {Lystbæk, Mathias N. and Mikkelsen, Thorbjørn and Krisztandl, Roland and Gonzalez, Eric J. and Gonzalez-Franco, Mar and Gellersen, Hans and Pfeuffer, Ken},
    year         = 2024,
    booktitle    = {Proceedings of the 37th Annual ACM Symposium on User Interface Software and Technology},
    location     = {Pittsburgh, PA, USA},
    publisher    = {Association for Computing Machinery},
    address      = {New York, NY, USA},
    series       = {UIST '24},
    doi          = {10.1145/3654777.3676331},
    url          = {https://doi.org/10.1145/3654777.3676331},
    abstract     = {Interactions with objects in 3D frequently require complex manipulations beyond selection, hence gaze and pinch can fall short as a technique. Even simple drag and drop can benefit of further hand tracking, not to mention rotation of objects or bimanual formations to move multiple pieces or attach parts. Interactions of this type map naturally to the use of both hands for symmetric and asymmetric input, where framing — such a rotation — of the object by the non-dominant hand prepares the spatial reference in which the intended manipulation is performed by the dominant hand. In this work, we build on top of gaze and pinch, and explore gaze support for asymmetric bimanual input. With direct bimanual input as baseline, we consider three alternative conditions, where input by non-dominant, dominant, or both hands is indirect. We conduct a comparative study to evaluate the performance on an abstract rotate & manipulate task, revealing the merits and limitations of each method. We then implement our own learned guidelines on a series of demonstrative applications.},
    keywords     = {virtual reality, eye-tracking, gaze input, bimanual interaction, 3D manipulation}
}
```


# Using Hands-on, Hands-off Interaction

This repository contains the code for adding bimanual gaze-assisted interaction to Meta XR SDK.
The Unity project code contains an example scene with the interaction combinations already set up.

Before importing the GazeBimanualMetaSDK-v1.0 package, you will need the following dependencies from the Asset Store:
 - Meta XR All-in-One SDK (v62+)
 - Quick Outline (v1.1) (note that, overlapping outlines may not be visible)

If one wishes to add the interaction combinations to their project, simply import the "GazeBimanual-MetaSDK.unitypackage" in Unity 2022, then add the prefab "Gaze Bimanual Interaction" in "Assets\GazeBimanual\Prefabs" to the scene. 
After doing so, add an "IndirectGazeInteractable" component to a game object to enable indirect interaction (with Meta XR SDK "HandGrabInteractable" to enable direct interaction).
Then, switching between direct and indirect interaction is seamless and fully hand-agnostic.
