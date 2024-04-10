# Software Systems Group 1

## Physical Design

Within the `design` directory, you will find the SketchUp model files:
- `box_model.skp` for the primary structure of the box
- `lid_stop.skp` for the right side of the lid stop mechanism
- `lid_stop_left.skp` for the left side of the lid stop mechanism

These models are provided in both SketchUp format and STL (Stereolithography) format for 3D printing and rendering.

<img src="./img/readme_box_design.png" alt="Box Design" width="600"/>

*Note: The image above is a representation of the box design. To view the design in full detail, please access the SketchUp files in the `design` folder.*

## Electrical Architecture

Below is a Mermaid.js diagram illustrating the various connections within the hardware system necessary for its operation. It outlines the relationships between the power supplies, relays, peltier plate, and fans, as well as their connection to the Meadow.

```mermaid
graph LR
    subgraph 24V
    R[Relay 1] --> F1[Fan<br>24V]
    R --> F2[Fan<br>24V]
    R --> F3[Fan<br>24V]
    R --> F4[Fan<br>24V]
    end

    subgraph 12V
    R2[Relay 2] --> F6(Fan<br>12V, 0.12A)
    R2 --> F5(Fan<br>12V, 0.24A)
    R2 --> 1(Peltier Plate<br>12V)
    end

    v12[12V Current Transformer] --> C12[Clamp]
    v24[24V Current Transformer] --> C24[Clamp]
    
    C12 --> R2
    M[Meadow] --> R
    M[Meadow] --> R2
    C24 --> R
```
