--	General camera parameters
FieldOfView = 90.000000
ClipNear = 1
ClipFar = 9999

TerrainClip = 1.0

--	Interpolation in camera motion and camera snapping
--
--	Rate = rate to interpolate
--	Base = base to perform interpolation in (2.71828 for linear, must be >1)
--	Threshold = threshold to start performing interpolation
--

--      Controls pan speed
SlideTargetRate = 5
SlideTargetBase = 5
--SlideTargetThreshold = 1

--      Controls zoom speed
SlideDistRate = 2
SlideDistBase = 5
--SlideDistThreshold = 1

--      Controls orbit speed
SlideOrbitRate = 4
SlideOrbitBase = 1.01
--SlideOrbitThreshold = 1

--      Controls declination speed
SlideDeclRate = 4
SlideDeclBase = 1.01
--SlideDeclThreshold = 1

--	Controls the speed of the zoom with the double button press
DistRateMouse = 0.50

--	Controls the speed of the zoom on the wheel
DistRateWheelZoomIn = 0.7
DistRateWheelZoomOut = 1.3

--	Distance range in metres
DistMin = 2.0
DistMax = 100.0

--	Declination speed
DeclRateMouse = -5

--	Declination range : angle range you can look at a target
DeclMin = 2.0
DeclMax = 200.0

--	Mouse orbit speed
OrbitRateMouse = -4


--	Default camera parameters
DefaultDistance = 55
DefaultDeclination = 45
DefaultOrbit = 45

--	Minimum eye height
DistGroundHeight = 1.0


--	Pan velocity scaling
--	Panning speed at the default/min height
PanScaleMouseDefZ = 500

PanScaleKeyboardDefZ = 175
PanScaleKeyboardMinZ = 35

PanScaleScreenDefZ = 150
PanScaleScreenMinZ = 30

-- Panning acceleration
-- To turn acceleration off, use the following values:
--     PanAccelerate = 0.0
--     PanStartSpeedScalar = 1.0
PanAccelerate = 0.0
PanStartSpeedScalar = 0.5
PanMaxSpeedScalar = 1.0


--	Enable/disable declination
DeclinationEnabled = 1.0

--	Enable/disable rotation
OrbitEnabled = 1.0

