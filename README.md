# 2DFPhysics
 2D fixed-point physics for Unity. This is currently a WIP project, any help is appriciated.    
 This project is currently based on Randy Gaul's [Custom 2D Physics Engine tutorial](https://gamedevelopment.tutsplus.com/tutorials/how-to-create-a-custom-2d-physics-engine-the-basics-and-impulse-resolution--gamedev-6331), box2D, and various other resources.  
 Currently built on Unity **2019.2.3f1**, but could work on lower versions.
 
## Goals
1. Deterministic. Should get the same result on all platforms. 
2. Supports rollback. Should be able to save/restore anything having to do with the simulation and have it play back the exact same.

## Resources
* [The Great Divide: Unique Visuals and Deterministic Gameplay in Deserts of Kharak](https://www.youtube.com/watch?v=wwLW6CjswxM)  
* [How to Create a Custom 2D Physics Engine](https://gamedevelopment.tutsplus.com/tutorials/how-to-create-a-custom-2d-physics-engine-the-basics-and-impulse-resolution--gamedev-6331)  
* [Broad Phase using Spatial Partitioning](http://buildnewgames.com/broad-phase-collision-detection/)   


## Other Details
* The project uses [my fork of FixedPointy](https://github.com/christides11/FixedPointy) to handle all the fixed point math, I'd appreciate any contributions there also.
