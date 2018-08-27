A Vive SRWorks Hand Interaction example for Unity

The example sceme shows how to occlude and interact with virtual objects with your hands

- In this example, by default the occlusion is using a depth mask (you can enable/disable it in SRWorksHand)

- You can also view the hand's mesh instead of the occlusion depth mask by enabling it in SRWorksHand

- To slap/punch (without going through) enable the HandRacketCollisionObj in the scene 
  (you can make it visible by enabling the Mesh Renderer on it)

- Depth mask occlusion settings can be edited at runtime in the inspector on the DualCamera (head) object 
  in addition to the Game Window SRWorks settings that can be called up by pressing the S and R keys.

Requirements: 
- VIVE Pro (make sure cameras are enabled and working in settings)
- ViveSRWorks Unity package: Vive-SRWorks-0.7.5.0-Unity-Plugin.unitypackage (install this first)
