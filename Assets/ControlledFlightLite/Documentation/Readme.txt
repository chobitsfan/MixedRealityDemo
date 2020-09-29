Controlled Flight Lite - Unity Asset

Website documentation: https://sparsedesign.com/controlled-flight/
Youtube: https://www.youtube.com/channel/UCKnYiftmgSz8gqtQTatWwew
Support: controlledflight@sparsedesign.com

When using HDRP or URP, unpack the materials in the relevant package in the
Materials folder.

Video tutorial: https://www.youtube.com/watch?v=DJBoVVlFHvU

Use of Controlled flight:
Basic functionality, and most settings, by adding the MissileSupervisor 
component to a Game Object. Each setting in the component have a tooltip.
All components can be found in the Controlled Flight folder in the Component menu

Target guidance:
1. Create Game Object to control.
2. Add MissileSupervisor component.
3. In the component, set Target Type to TARGET (should be default).
4. Drag target to Target Field.

This is enough for the object to intercept the target (if possible).
See https://sparsedesign.com/controlled-flight/target-guidance/

Waypoint guidance:
1. Create Game Object to control.
2. Add MissileSupervisor component.
3. In the component, set Target Type to PATH.
   --- (Only full version)
   --- (4a. Select Path Type as OBJECT to use Game objects as waypoints (often easier than coordinates).)
   --- (4a. Select Path Type as COORDS to input waypoints as coordinates.)
5. Input waypoints as in Game Objects in the Path Objects list
   --- (or input coordinates in the Path Coordinates list.)

This is enough for the object to follow a set of waypoints.
See https://sparsedesign.com/controlled-flight/path-following/
