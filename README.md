# FALCO
A VR visualizer for protein-protein interaction (and other) networks built in the Unity engine.
## Hardware Requirements
A dedicated GPU is recommended for the mouse and keyboard scene, and required for VR visualization.  FALCON was developed on a computer with the following specifications:
+ AMD Ryzen 7 3700X
+ NVIDIA GTX 1060 6GB
+ 16 GB DDR4 RAM
+ Windows 10 64-bit Operaing System
## Installation
In order to install FALCON, download one of the [releases](https://github.com/ewbell94/FALCON/releases) and run the binary.  On Mac OS X, you may need to change the permissions of the application in the terminal to allow it to run:
```
spctl --master-disable
sudo xattr -rd com.apple.quarantine FALCON.app
```
## Controls
### VR Controls
![alt text](https://github.com/ewbell94/FALCON/blob/main/Assets/Textures/VRControl.png)
### Mouse and Keyboard Controls
![alt text](https://github.com/ewbell94/FALCON/blob/main/Assets/Textures/MKControl.png)
## Supported File Formats
### Simple Interaction Format (SIF)
This file format is taken from Cytoscape, a line-delimited list of interactions where interacting elements are separated by a token describing their interaction type (e.g. "pp" for protein-protein).
```
prot1 pp prot2
prot2 pp prot3 pp prot4
prot5 pp prot6
prot6 pp prot7
...
```
### Comma Separated Values (CSV)
For edge weighting and node renaming, a comma separated values file is supported.  For node renaming, each line should consist of the old name and the new name.  (NOTE: node renaming completely removes the old names of the nodes, so if you have additional files referencing the nodes by the old names, perform the node renaming after you use those files.)
```
prot2089,SEPTIN4
prot2090,DYNC1LI2
prot2091,KLK10
prot2092,PSMD3
prot2093,SLC7A4
prot2094,TEX33
prot2095,HOXC11
prot2096,RBFOX2
prot2097,PAPSS1
prot2098,SIAH2
...
```
For edge weighting, each line should consist of the interaction of interest (the two interactors separated by a dash), followed by the weight value.  These values are linearly normalized when used to weight the edges.
```
prot16920-prot20601,3.352
prot9236-prot20619,2.046
prot12444-prot20619,1.695
prot8105-prot20619,1.618
prot4246-prot20617,1.529
prot102-prot20617,1.529
prot1445-prot20617,1.526
prot17213-prot20619,1.475
prot1445-prot20603,1.453
prot103-prot20603,1.445
...
```
