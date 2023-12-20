'From Squeak3.6 of ''6 October 2003'' [latest update: #5429] on 7 February 2004 at 5:00:48 pm'!!LineSegment methodsFor: 'accessing' stamp: 'ar 6/8/2003 00:07'!degree	^1! !!LineSegment methodsFor: 'accessing' stamp: 'ar 6/7/2003 00:10'!end: aPoint	end _ aPoint! !!LineSegment methodsFor: 'accessing' stamp: 'ar 6/7/2003 00:10'!start: aPoint	start _ aPoint! !!LineSegment methodsFor: 'testing' stamp: 'ar 6/8/2003 01:03'!isArcSegment	"Answer whether I approximate an arc segment reasonably well"	| mid v1 v2 d1 d2 center |	start = end ifTrue:[^false].	mid := self valueAt: 0.5.	v1 := (start + mid) * 0.5.	v2 := (mid + end) * 0.5.	d1 := mid - start. d1 := d1 y @ d1 x negated.	d2 := end - mid.  d2 := d2 y @ d2 x negated.	center := LineSegment		intersectFrom: v1 with: d1 to: v2 with: d2.	"Now see if the tangents are 'reasonably close' to the circle"	d1 := (start - center) normalized dotProduct: self tangentAtStart normalized.	d1 abs > 0.02 ifTrue:[^false].	d1 := (end - center) normalized dotProduct: self tangentAtEnd normalized.	d1 abs > 0.02 ifTrue:[^false].	d1 := (mid - center) normalized dotProduct: self tangentAtMid normalized.	d1 abs > 0.02 ifTrue:[^false].	^true! !!LineSegment methodsFor: 'vector functions' stamp: 'ar 6/7/2003 00:15'!asBezier2Curves: err	^Array with: self! !!LineSegment methodsFor: 'vector functions' stamp: 'ar 6/7/2003 23:48'!curveFrom: parameter1 to: parameter2	"Create a new segment like the receiver but starting/ending at the given parametric values"	| delta |	delta _ end - start.	^self clone from: delta * parameter1 + start to: delta * parameter2 + start! !!LineSegment methodsFor: 'vector functions' stamp: 'ar 6/8/2003 00:54'!tangentAtMid	"Return the tangent for the last point"	^(end - start)! !!LineSegment methodsFor: 'converting' stamp: 'ar 6/8/2003 04:19'!asBezier2Points: error	^Array with: start with: start with: end! !!LineSegment methodsFor: 'converting' stamp: 'ar 6/8/2003 15:38'!asBezier2Segments: error	"Demote a cubic bezier to a set of approximating quadratic beziers."	| pts |	pts _ self asBezier2Points: error.	^(1 to: pts size by: 3) collect:[:i| 		Bezier2Segment from: (pts at: i) via: (pts at: i+1) to: (pts at: i+2)].! !!LineSegment methodsFor: 'converting' stamp: 'ar 6/7/2003 20:57'!asTangentSegment	^LineSegment from: end-start to: end-start! !!LineSegment methodsFor: 'converting' stamp: 'ar 6/7/2003 00:08'!reversed	^self class controlPoints: self controlPoints reversed! !!LineSegment methodsFor: 'private' stamp: 'ar 6/7/2003 21:00'!debugDraw	^self debugDrawAt: 0@0.! !!LineSegment methodsFor: 'private' stamp: 'ar 6/7/2003 21:00'!debugDrawAt: offset	| canvas |	canvas _ Display getCanvas.	canvas translateBy: offset during:[:aCanvas|		self lineSegmentsDo:[:p1 :p2|			aCanvas line: p1 rounded to: p2 rounded width: 1 color: Color black.		].	].! !!LineSegment methodsFor: 'bezier clipping' stamp: 'ar 6/8/2003 00:06'!bezierClipCurve: aCurve	^self bezierClipCurve: aCurve epsilon: 1! !!LineSegment methodsFor: 'bezier clipping' stamp: 'ar 6/8/2003 00:19'!bezierClipCurve: aCurve epsilon: eps	"Compute the intersection of the receiver (a line) with the given curve using bezier clipping."	| tMin tMax clip newCurve |	clip := self bezierClipInterval: aCurve.	clip ifNil:[^#()]. "no overlap"	tMin := clip at: 1.	tMax := clip at: 2.	newCurve := aCurve curveFrom: tMin to: tMax.	newCurve length < eps ifTrue:[^Array with: (aCurve valueAt: tMin + tMax * 0.5)].	(tMin < 0.001 and:[tMax > 0.999]) ifTrue:[		"Need to split aCurve before proceeding"		| curve1 curve2 |		curve1 := aCurve curveFrom: 0.0 to: 0.5.		curve2 := aCurve curveFrom: 0.5 to: 1.0.		^(curve1 bezierClipCurve: self epsilon: eps),			(curve2 bezierClipCurve: self epsilon: eps).	].	^newCurve bezierClipCurve: self epsilon: eps.! !!LineSegment methodsFor: 'bezier clipping' stamp: 'ar 6/7/2003 23:58'!bezierClipInterval: aCurve	"Compute the new bezier clip interval for the argument,	based on the fat line (the direction aligned bounding box) of the receiver.	Note: This could be modified so that multiple clip intervals are returned.	The idea is that for a distance curve like			x		x	tMax----	--\-----/---\-------				x		x	tMin-------------------------	all the intersections intervals with tMin/tMax are reported, therefore	minimizing the iteration count. As it is, the process will slowly iterate	against tMax and then the curve will be split.	"	| nrm tStep pts eps inside vValue vMin vMax tValue tMin tMax 	last lastV lastT lastInside next nextV nextT nextInside |	eps _ 0.00001.					"distance epsilon"	nrm _ (start y - end y) @ (end x - start x). "normal direction for (end-start)"	"Map receiver's control point into fat line; compute vMin and vMax"	vMin _ vMax _ nil.	self controlPointsDo:[:pt|		vValue _ (nrm x * pt x) + (nrm y * pt y). "nrm dotProduct: pt."		vMin == nil	ifTrue:[	vMin _ vMax _ vValue]					ifFalse:[vValue < vMin ifTrue:[vMin _ vValue].							vValue > vMax ifTrue:[vMax _ vValue]]].	"Map the argument into fat line; compute tMin, tMax for clip"	tStep _ 1.0 / aCurve degree.	pts _ aCurve controlPoints.	last _ pts at: pts size.	lastV _ (nrm x * last x) + (nrm y * last y). "nrm dotProduct: last."	lastT _ 1.0.	lastInside _ lastV+eps < vMin ifTrue:[-1] ifFalse:[lastV-eps > vMax ifTrue:[1] ifFalse:[0]].	"Now compute new minimal and maximal clip boundaries"	inside _ false.	"assume we're completely outside"	tMin _ 2.0. tMax _ -1.0. 	"clip interval"	1 to: pts size do:[:i|		next _ pts at: i.		nextV _ (nrm x * next x) + (nrm y * next y). "nrm dotProduct: next."		false ifTrue:[			(nextV - vMin / (vMax - vMin)) printString displayAt: 0@ (i-1*20)].		nextT _ i-1 * tStep.		nextInside _ nextV+eps < vMin ifTrue:[-1] ifFalse:[nextV-eps > vMax ifTrue:[1] ifFalse:[0]].		nextInside = 0 ifTrue:[			inside _ true.			tValue _ nextT.			tValue < tMin ifTrue:[tMin _ tValue].			tValue > tMax ifTrue:[tMax _ tValue].		].		lastInside = nextInside ifFalse:["At least one clip boundary"			inside _ true.			"See if one is below vMin"			(lastInside + nextInside <= 0) ifTrue:[				tValue _ lastT + ((nextT - lastT) * (vMin - lastV) / (nextV - lastV)).				tValue < tMin ifTrue:[tMin _ tValue].				tValue > tMax ifTrue:[tMax _ tValue].			].			"See if one is above vMax"			(lastInside + nextInside >= 0) ifTrue:[				tValue _ lastT + ((nextT - lastT) * (vMax - lastV) / (nextV - lastV)).				tValue < tMin ifTrue:[tMin _ tValue].				tValue > tMax ifTrue:[tMax _ tValue].			].		].		last _ next.		lastT _ nextT.		lastV _ nextV.		lastInside _ nextInside.	].	inside		ifTrue:[^Array with: tMin with: tMax]		ifFalse:[^nil]! !!Bezier2Segment methodsFor: 'initialize' stamp: 'ar 6/7/2003 22:37'!from: startPoint to: endPoint withMidPoint: pointOnCurve	"Initialize the receiver with the pointOnCurve assumed at the parametric value 0.5"	start _ startPoint.	end _ endPoint.	"Compute via"	via _ (pointOnCurve * 2) - (start + end * 0.5).! !!Bezier2Segment methodsFor: 'initialize' stamp: 'ar 6/6/2003 03:03'!from: startPoint to: endPoint withMidPoint: pointOnCurve at: parameter	"Initialize the receiver with the pointOnCurve at the given parametric value"	| t1 t2 t3 |	start _ startPoint.	end _ endPoint.	"Compute via"	t1 _ (1.0 - parameter) squared.	t2 _ 1.0 / (2 * parameter * (1.0 - parameter)).	t3 _ parameter squared.	via _ (pointOnCurve - (start * t1)  - (end * t3)) * t2! !!Bezier2Segment methodsFor: 'accessing' stamp: 'ar 6/8/2003 00:07'!degree	^2! !!Bezier2Segment methodsFor: 'vector functions' stamp: 'ar 6/8/2003 00:08'!curveFrom: param1 to: param2	"Return a new curve from param1 to param2"	| newStart newEnd newVia tan1 tan2 d1 d2 |	tan1 := via - start.	tan2 := end - via.	param1 <= 0.0 ifTrue:[		newStart _ start.	] ifFalse:[		d1 := tan1 * param1 + start.		d2 := tan2 * param1 + via.		newStart := (d2 - d1) * param1 + d1	].	param2 >= 1.0 ifTrue:[		newEnd _ end.	] ifFalse:[		d1 := tan1 * param2 + start.		d2 := tan2 * param2 + via.		newEnd := (d2 - d1) * param2 + d1.	].	tan2 := (tan2 - tan1 * param1 + tan1) * (param2 - param1).	newVia := newStart + tan2.	^self clone from: newStart to: newEnd via: newVia.! !!Bezier2Segment methodsFor: 'vector functions' stamp: 'ar 6/7/2003 00:04'!outlineSegment: width	| delta newStart newEnd param newMid |	delta := self tangentAtStart normalized * width.	delta := delta y @ delta x negated.	newStart := start + delta.	delta := self tangentAtEnd normalized * width.	delta := delta y @ delta x negated.	newEnd := end + delta.	param := 0.5. "self tangentAtStart r / (self tangentAtStart r + self tangentAtEnd r)."	delta := (self tangentAt: param) normalized * width.	delta := delta y @ delta x negated.	newMid := (self valueAt: param) + delta.	^self class from: newStart to: newEnd withMidPoint: newMid at: param! !!Bezier2Segment methodsFor: 'vector functions' stamp: 'ar 6/9/2003 03:43'!parameterAtExtreme: tangentDirection	"Compute the parameter value at which the tangent reaches tangentDirection.	We need to find the parameter value t at which the following holds		((t * dir + in) crossProduct: tangentDirection) = 0.	Since this is pretty ugly we use the normal direction rather than the tangent and compute the equivalent relation using the dot product as		((t * dir + in) dotProduct: nrm) = 0.	Reformulation yields		((t * dir x + in x) * nrm x) + ((t * dir y + in y) * nrm y) = 0.		(t * dir x * nrm x) + (in x * nrm x) + (t * dir y * nrm y) + (in y * nrm y) = 0.		(t * dir x * nrm x) + (t * dir y * nrm y) = 0 - ((in x * nrm x) + (in y * nrm y)).				(in x * nrm x) + (in y * nrm y)		t = 0 -	---------------------------------------			 	(dir x * nrm x) + (dir y * nrm y)	And that's that. Note that we can get rid of the negation by computing 'dir' the other way around (e.g., in the above it would read '-dir') which is trivial to do. Note also that the above does not generalize easily beyond 2D since its not clear how to express the 'normal direction' of a tangent plane.	"	| inX inY dirX dirY nrmX nrmY |	"Compute in"	inX _ via x - start x.	inY _ via y - start y.	"Compute -dir"	dirX _ inX - (end x - via x).	dirY _ inY - (end y - via y).	"Compute nrm"	nrmX _ tangentDirection y.	nrmY _ 0 - tangentDirection x.	"Compute result"	^((inX * nrmX) + (inY * nrmY)) / 		((dirX * nrmX) + (dirY * nrmY))! !!Bezier2Segment methodsFor: 'vector functions' stamp: 'ar 6/9/2003 03:43'!parameterAtExtremeX	"Note: Only valid for non-monoton receivers"	^self parameterAtExtreme: 0.0@1.0.! !!Bezier2Segment methodsFor: 'vector functions' stamp: 'ar 6/9/2003 03:43'!parameterAtExtremeY	"Note: Only valid for non-monoton receivers"	^self parameterAtExtreme: 1.0@0.0.! !!Bezier2Segment methodsFor: 'vector functions' stamp: 'ar 6/8/2003 00:54'!tangentAtMid	"Return the tangent at the given parametric value along the receiver"	| in out |	in _ self tangentAtStart.	out _ self tangentAtEnd.	^in + out * 0.5! !!Bezier2Segment methodsFor: 'converting' stamp: 'ar 6/8/2003 04:19'!asBezier2Points: error	^Array with: start with: via with: end! !!Bezier2Segment methodsFor: 'converting' stamp: 'ar 6/7/2003 21:05'!asBezier3Segment	"Represent the receiver as cubic bezier segment"	^Bezier3Segment		from: start		via: 2*via+start / 3.0		and: 2*via+end / 3.0		to: end! !!Bezier2Segment methodsFor: 'converting' stamp: 'ar 6/7/2003 20:58'!asTangentSegment	^LineSegment from: via-start to: end-via! !!Bezier2Segment methodsFor: 'bezier clipping' stamp: 'ar 6/7/2003 23:45'!bezierClipHeight: dir	| dirX dirY uMin uMax dx dy u |	dirX _ dir x.	dirY _ dir y.	uMin _ 0.0.	uMax _ (dirX * dirX) + (dirY * dirY).	dx _ via x - start x.	dy _ via y - start y.	u _ (dirX * dx) + (dirY * dy).	u < uMin ifTrue:[uMin _ u].	u > uMax ifTrue:[uMax _ u].	^uMin@uMax! !!Bezier3Segment methodsFor: 'accessing' stamp: 'ar 6/8/2003 00:07'!degree	^3! !!Bezier3Segment methodsFor: 'accessing' stamp: 'ar 6/6/2003 22:37'!via1	^via1! !!Bezier3Segment methodsFor: 'accessing' stamp: 'ar 6/6/2003 22:37'!via2	^via2! !!Bezier3Segment methodsFor: 'converting' stamp: 'ar 6/7/2003 21:07'!asBezier2Points: error	"Demote a cubic bezier to a set of approximating quadratic beziers.	Should convert to forward differencing someday"	| curves pts step prev index a b f |	curves _ self bezier2SegmentCount: error.	pts _ Array new: curves * 3.	step _ 1.0 / (curves * 2).	prev _ start.	1 to: curves do: [ :c |		index _ 3*c.		a _ pts at: index-2 put: prev.		b _ (self valueAt: (c*2-1)*step).		f _ pts at: index put: (self valueAt: (c*2)*step).		pts at: index-1 put: (4 * b - a - f) / 2.		prev _ pts at: index.		].	^ pts.	! !!Bezier3Segment methodsFor: 'converting' stamp: 'ar 6/7/2003 21:07'!asBezier2Segments	"Demote a cubic bezier to a set of approximating quadratic beziers."	^self asBezier2Segments: 0.5! !!Bezier3Segment methodsFor: 'converting' stamp: 'ar 6/6/2003 22:23'!asBezierShape	"Demote a cubic bezier to a set of approximating quadratic beziers."	^self asBezierShape: 0.5! !!Bezier3Segment methodsFor: 'converting' stamp: 'ar 6/7/2003 21:09'!asBezierShape: error	"Demote a cubic bezier to a set of approximating quadratic beziers.	Should convert to forward differencing someday"	^(self asBezier2Points: error) asPointArray.! !!Bezier3Segment methodsFor: 'converting' stamp: 'ar 6/7/2003 20:58'!asTangentSegment	^Bezier2Segment 		from: via1-start 		via: via2-via1		to: end-via2! !!Bezier3Segment methodsFor: 'bezier clipping' stamp: 'ar 6/7/2003 23:45'!bezierClipHeight: dir	"Check if the argument overlaps the receiver somewhere 	along the line from start to end. Optimized for speed."	| u dirX dirY dx dy uMin uMax |	dirX _ dir x.	dirY _ dir y.	uMin _ 0.0.	uMax _ (dirX * dirX) + (dirY * dirY).	dx _ via1 x - start x.	dy _ via1 y - start y.	u _ (dirX * dx) + (dirY * dy).	u < uMin ifTrue:[uMin _ u].	u > uMax ifTrue:[uMax _ u].	dx _ via2 x - start x.	dy _ via2 y - start y.	u _ (dirX * dx) + (dirY * dy).	u < uMin ifTrue:[uMin _ u].	u > uMax ifTrue:[uMax _ u].	^uMin@uMax! !!Bezier3Segment methodsFor: 'vector functions' stamp: 'ar 6/7/2003 00:04'!outlineSegment: width	| tan1 nrm1 tan2 nrm2 newStart newVia1 newEnd newVia2 dist |	tan1 _ (via1 - start) normalized.	nrm1 _ tan1 * width.	nrm1 _ nrm1 y @ nrm1 x negated.	tan2 _ (end - via2) normalized.	nrm2 _ tan2 * width.	nrm2 _ nrm2 y @ nrm2 x negated.	newStart _ start + nrm1.	newEnd _ end + nrm2.	dist _ (newStart dist: newEnd) * 0.3.	newVia1 _ newStart + (tan1 * dist).	newVia2 _ newEnd - (tan2 * dist).	^self class from: newStart via: newVia1 and: newVia2 to: newEnd.! !!Bezier3Segment methodsFor: 'vector functions' stamp: 'ar 6/7/2003 19:25'!tangentAt: parameter	| tan1 tan2 tan3 t1 t2 t3 |	tan1 := via1 - start.	tan2 := via2 - via1.	tan3 := end - via2.	t1 _ (1.0 - parameter) squared.	t2 _ 2 * parameter * (1.0 - parameter).	t3 _ parameter squared.	^(tan1 * t1) + (tan2 * t2) + (tan3 * t3)! !!Bezier3Segment methodsFor: 'vector functions' stamp: 'ar 6/6/2003 22:02'!tangentAtEnd	^end - via2! !!Bezier3Segment methodsFor: 'vector functions' stamp: 'ar 6/8/2003 00:56'!tangentAtMid	| tan1 tan2 tan3 |	tan1 := via1 - start.	tan2 := via2 - via1.	tan3 := end - via2.	^(tan1 + (2*tan2) + tan3) * 0.25! !!Bezier3Segment methodsFor: 'vector functions' stamp: 'ar 6/6/2003 22:01'!tangentAtStart	^via1 - start! !!LineSegment class methodsFor: 'utilities' stamp: 'ar 6/8/2003 00:49'!intersectFrom: startPt with: startDir to: endPt with: endDir	"Compute the intersection of two lines, e.g., compute alpha and beta for		startPt + (alpha * startDir) = endPt + (beta * endDir).	Reformulating this yields		(alpha * startDir) - (beta * endDir) = endPt - startPt.	or		(alpha * startDir) + (-beta * endDir) = endPt - startPt.	or		(alpha * startDir x) + (-beta * endDir x) = endPt x - startPt x.		(alpha * startDir y) + (-beta * endDir y) = endPt y - startPt y.	which is trivial to solve using Cramer's rule. Note that since	we're really only interested in the intersection point we need only	one of alpha or beta since the resulting intersection point can be	computed based on either one."	| det deltaPt alpha |	det _ (startDir x * endDir y) - (startDir y * endDir x).	det = 0.0 ifTrue:[^nil]. "There's no solution for it"	deltaPt _ endPt - startPt.	alpha _ (deltaPt x * endDir y) - (deltaPt y * endDir x).	alpha _ alpha / det.	"And compute intersection"	^startPt + (alpha * startDir)! !