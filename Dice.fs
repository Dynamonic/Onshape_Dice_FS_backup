FeatureScript 1634;
import(path : "onshape/std/geometry.fs", version : "1634.0");

annotation { "Feature Type Name" : "Dice", "Feature Name Template" : "Dice", "Editing Logic Function" : "editDiceLogic" }
export const Dice = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name": "Number of Sides" }
        definition.DiceType is DiceType;
        
        if(definition.DiceType == DiceType.d4){
            annotation { "Name": "D4 Type"}
            definition.D4_Type is D4Type;
        }
        if(definition.DiceType == DiceType.d12){
            annotation{"Name": "D12 Type"}
            definition.D12_Type is D12Type;
        }
        
        annotation { "Name" : "Center point", "Filter" : EntityType.VERTEX, "MaxNumberOfPicks" : 1 }
        definition.centerpoint is Query;
        
        annotation { "Name": "Scale (Based on D6 side length)"}
        isLength(definition.size, SIZE_BOUNDS);

    }
    
    {
        //Currently makes dice on or about origin
        
        var location = vector(0, 0, 0) * millimeter+evVertexPoint(context, {
                        "vertex" : definition.centerpoint
                    });
        var sketchPlane = opPlane(context, id+"start", {
            "plane": plane(location, vector(0, 0, 1), vector(1, 0, 0))
            });
        
        
        //println(location[0]);
        //println(location[1]);
        //println(location[2]);
        //add D4 Here
        if(definition.DiceType == DiceType.d4)
        {
            const d4_id = id+"d4";
            const d4_side = (20/16)*(definition.size);
            
            if(definition.D4_Type == D4Type.reg){
                const numPlanes = 4;
                var vertex1 = location - (d4_side/2)*vector(1,0,0) - (d4_side/(2*sqrt(3)))*vector(0,1,0);
                var vertex2 = location + (d4_side/2)*vector(1,0,0) - (d4_side/(2*sqrt(3)))*vector(0,1,0);
                var vertex3 = location + (d4_side/sqrt(3))*vector(0,1,0);
                var vertex4 = location + ((d4_side*sqrt(6))/3)*vector(0,0,1);
                var points = [vertex1, vertex2, vertex3, vertex4];
                //make 4 planes
                var face = [];
                const planeId = d4_id + "plane";
                var area = definition.size^2;
                for(var i = 0;i<numPlanes;i+=1)
                {
                    face = [points[i],points[(i+1)%numPlanes],points[(i+2)%numPlanes]];
                    planeCalc(context, planeId+i, face, "plane"~i, area);
                }
                
                //operation for making enclosed part from planes
                enclose(context, d4_id+"enclose1", {
                        "entities": qCreatedBy(planeId, EntityType.BODY)
                });
            }
            if(definition.D4_Type == D4Type.crystal)
            {
                const d4_crystal = d4_id+"crystal";
                var sketchPlane1 = plane(location, vector(0, 0, 1), vector(1, 0, 0));
                const center = worldToPlane(sketchPlane1, location);
                const square = [center-(d4_side/4)*vector(1,0) - (d4_side/4)*vector(0,1),
                                center-(d4_side/4)*vector(1,0) + (d4_side/4)*vector(0,1),
                                center+(d4_side/4)*vector(1,0) + (d4_side/4)*vector(0,1),
                                center+(d4_side/4)*vector(1,0) - (d4_side/4)*vector(0,1)];
                const tips = [location + (5*(d4_side/8))*vector(0,0,1),
                              location - (5*(d4_side/8))*vector(0,0,1)];
                opPoint(context, d4_crystal+"tip1",{
                "point" : tips[0]});
                opPoint(context, d4_crystal+"tip2",{
                "point" : tips[1]});
                var sketchPlane2 = plane(location+(d4_side/3)*vector(0,0,1), vector(0, 0, 1), vector(1, 0, 0));
                var sketchPlane3 = plane(location-(d4_side/3)*vector(0,0,1), vector(0, 0, 1), vector(1, 0, 0));
                const squareSketch1 = newSketchOnPlane(context, d4_crystal + "squareSketch1", { "sketchPlane" : sketchPlane2 });
                for (var i = 0; i < size(square); i += 1)
                {
                    skLineSegment(squareSketch1, "line" ~ i, { "start" : square[i], "end" : square[(i + 1) % size(square)] });
                }
                skSolve(squareSketch1);
                const squareSketch2 = newSketchOnPlane(context, d4_crystal + "squareSketch2", { "sketchPlane" : sketchPlane3 });
                for (var i = 0; i < size(square); i += 1)
                {
                    skLineSegment(squareSketch2, "line" ~ i, { "start" : square[i], "end" : square[(i + 1) % size(square)] });
                }
                skSolve(squareSketch2);
                opLoft(context, d4_crystal + "loft"+"1", {
                        "profileSubqueries" : [ qCreatedBy(d4_crystal + "squareSketch1", EntityType.FACE), qCreatedBy(d4_crystal + "tip1", EntityType.VERTEX) ],
                });
                opLoft(context, d4_crystal + "loft"+"2", {
                        "profileSubqueries" : [ qCreatedBy(d4_crystal + "squareSketch2", EntityType.FACE), qCreatedBy(d4_crystal + "tip2", EntityType.VERTEX) ],
                });
                opLoft(context, d4_crystal + "loft"+"3", {
                        "profileSubqueries" : [ qCreatedBy(d4_crystal + "squareSketch1", EntityType.FACE), qCreatedBy(d4_crystal + "squareSketch2", EntityType.FACE) ],
                });
                opBoolean(context, d4_crystal + "faceBoolean", {
                    "tools" : qCreatedBy(d4_crystal+"loft", EntityType.BODY),
                    "operationType" : BooleanOperationType.UNION
                });
                //loft sketch to respective point
                //boolean add
                
            }
            if(definition.D4_Type == D4Type.shard)
            {
                const d4_shard = d4_id+"shard";
                var sketchPlane1 = plane(location, vector(0, 0, 1), vector(1, 0, 0));
                const center = worldToPlane(sketchPlane1, location);
                const square = [center-(d4_side/3)*vector(1,0) - (d4_side/3)*vector(0,1),
                                center-(d4_side/3)*vector(1,0) + (d4_side/3)*vector(0,1),
                                center+(d4_side/3)*vector(1,0) + (d4_side/3)*vector(0,1),
                                center+(d4_side/3)*vector(1,0) - (d4_side/3)*vector(0,1)];
                const tips = [location + (3*(d4_side)/4)*vector(0,0,1),
                              location - (3*(d4_side/8))*vector(0,0,1)];
                opPoint(context, d4_shard+"tip1",{
                "point" : tips[0]});
                opPoint(context, d4_shard+"tip2",{
                "point" : tips[1]});
                const squareSketch = newSketchOnPlane(context, d4_shard + "squareSketch", { "sketchPlane" : sketchPlane1 });
                for (var i = 0; i < size(square); i += 1)
                {
                    skLineSegment(squareSketch, "line" ~ i, { "start" : square[i], "end" : square[(i + 1) % size(square)] });
                }
                skSolve(squareSketch);
                opLoft(context, d4_shard + "loft"+"1", {
                        "profileSubqueries" : [ qCreatedBy(d4_shard + "squareSketch", EntityType.FACE), qCreatedBy(d4_shard + "tip1", EntityType.VERTEX) ],
                });
                opLoft(context, d4_shard + "loft"+"2", {
                        "profileSubqueries" : [ qCreatedBy(d4_shard + "squareSketch", EntityType.FACE), qCreatedBy(d4_shard + "tip2", EntityType.VERTEX) ],
                });
                opBoolean(context, d4_shard + "faceBoolean", {
                    "tools" : qCreatedBy(d4_shard+"loft", EntityType.BODY),
                    "operationType" : BooleanOperationType.UNION
                });
            }
            
        }
        if(definition.DiceType == DiceType.d6)
        {
            const d6_id = id+"d6";
            var sketchPlane1 = plane(location, vector(0, 0, 1), vector(1, 0, 0));
            const d6_side = definition.size;
            const center = worldToPlane(sketchPlane1, location);
            var keyVector = vector(0, 1);
            var perpKeyVector = vector(-1, 0);
            var points = [
                center - (d6_side/2) * perpKeyVector - (d6_side/2) * keyVector,
                center - (d6_side / 2) * perpKeyVector + (d6_side/2) * keyVector,
                center + (d6_side / 2) * perpKeyVector + (d6_side/2) * keyVector,
                center + (d6_side / 2) * perpKeyVector - (d6_side/2) * keyVector
            ];
            const squareSketch = newSketchOnPlane(context, d6_id + "squareSketch", { "sketchPlane" : sketchPlane1 });
            for (var i = 0; i < size(points); i += 1)
                {
                    skLineSegment(squareSketch, "line" ~ i, { "start" : points[i], "end" : points[(i + 1) % size(points)] });
                }
            skSolve(squareSketch);
            opExtrude(context, d6_id + "extrude1", {
                    "entities" : qSketchRegion(d6_id + "squareSketch", true),
                    "direction" : sketchPlane1.normal,
                    "endBound" : BoundingType.BLIND,
                    "endDepth" : d6_side
            });
        }
        if(definition.DiceType == DiceType.d8)
        {
            const d8_id = id+"d8";
            var sketchPlane1 = plane(location, vector(0, 0, 1), vector(1, 0, 0));
            const numFaces = 8;
            const d8_side = (18/16)*definition.size;
            const squareSketch = newSketchOnPlane(context, d8_id + "squareSketch", { "sketchPlane" : sketchPlane1 });
            const center = worldToPlane(sketchPlane1, location);
            var keyVector = vector(0, 1);
            var perpKeyVector = vector(-1, 0);
            var points = [
                center - (d8_side/2) * perpKeyVector - (d8_side/2) * keyVector,
                center - (d8_side / 2) * perpKeyVector + (d8_side/2) * keyVector,
                center + (d8_side / 2) * perpKeyVector + (d8_side/2) * keyVector,
                center + (d8_side / 2) * perpKeyVector - (d8_side/2) * keyVector
            ];
            for (var i = 0; i < size(points); i += 1)
                {
                    skLineSegment(squareSketch, "line" ~ i, { "start" : points[i], "end" : points[(i + 1) % size(points)] });
                }
            skSolve(squareSketch);
            
            var tips = [
                location + ((d8_side/sqrt(2))*vector(0,0,1)),
                location - ((d8_side/sqrt(2))*vector(0,0,1))
            ];
            opPoint(context, d8_id+"tip1",{
                "point" : tips[0]});
            opPoint(context, d8_id+"tip2",{
                "point" : tips[1]});
            opLoft(context, d8_id+"loft1", {
                "profileSubqueries" : [
                    qCreatedBy(d8_id+"squareSketch", EntityType.FACE), 
                    qCreatedBy(d8_id+"tip1", EntityType.VERTEX)] 
            });
            opLoft(context, d8_id+"loft2", {
                "profileSubqueries" : [
                    qCreatedBy(d8_id+"squareSketch", EntityType.FACE), 
                    qCreatedBy(d8_id+"tip2", EntityType.VERTEX)] 
            });
            opBoolean(context, d8_id + "faceBoolean", {
                    "tools" : qUnion([qCreatedBy(d8_id+"loft1", EntityType.BODY),
                                    qCreatedBy(d8_id+"loft2", EntityType.BODY)]),
                    "operationType" : BooleanOperationType.UNION
                });
        }
        if(definition.DiceType == DiceType.d10)
        {   
            const d10_id = id+"d10";
            const d10_side = (15/16)*(definition.size);
            const d10_overall = (22/16)*definition.size;
            const d10_overlap = (3/15)*d10_side;
            const numFaces = 10;
            const tips = [location+(d10_overall/2)*vector(0,0,1), location-(d10_overall/2)*vector(0,0,1)];
            const pointz = (d10_overlap/2);
            var area = definition.size^2;

            const xyLength = d10_side*cos(asin(((d10_overall/2)-(d10_overlap/2))/d10_side));
            const pyrmag = (xyLength*d10_overall)/((d10_overall/2)-(d10_overlap/2));
            const topHalf = [
                location+((d10_overall/2)*vector(0,0,-1))+(pyrmag*vector(1,0,0)),
                location+((d10_overall/2)*vector(0,0,-1))+(pyrmag*vector(cos(72*degree),sin(72*degree),0)),
                location+((d10_overall/2)*vector(0,0,-1))+(pyrmag*vector(cos(144*degree),sin(144*degree),0)),
                location+((d10_overall/2)*vector(0,0,-1))+(pyrmag*vector(cos(216*degree),sin(216*degree),0)),
                location+((d10_overall/2)*vector(0,0,-1))+(pyrmag*vector(cos(288*degree),sin(288*degree),0)),
                ];
            planeCalc(context, d10_id+"pyr1",topHalf, "base", area);
            var sketch1 = newSketch(context, d10_id + "sketch"+"1", {
                            "sketchPlane" : qCreatedBy(d10_id+"pyr1"+"base", EntityType.FACE)
                });
            skRegularPolygon(sketch1, "polygon1", {
                    "center" : vector(location[0], -location[1]),
                    "firstVertex" : vector(pyrmag+location[0],-location[1]),
                    "sides" : 5
            });
            skSolve(sketch1);
            opPoint(context, d10_id+"tip",{
                "point" : tips[0]});
            opLoft(context, d10_id+"loft", {
                "profileSubqueries" : [
                    qCreatedBy(d10_id+"sketch"+"1", EntityType.FACE), 
                    qCreatedBy(d10_id+"tip", EntityType.VERTEX)] 
            });
            var sketchPlaneRot = opPlane(context, d10_id+"rotate", {
            "plane": plane(location, vector(0, 0, 1), vector(0, 1, 0))
            });
            var rotationPlane = newSketch(context, d10_id + "rotate2", {
                            "sketchPlane" : qCreatedBy(d10_id+"rotate", EntityType.FACE)
                });
            
            var rotateLine = skLineSegment(rotationPlane, "line1", {
                    "start" : vector(location[0],location[1]),
                    "end" : vector(location[0],location[1])+vector(sin(36*degree), cos(36*degree)) * inch
            });
            skSolve(rotationPlane);
            transform(context, d10_id+"copy1",{
                "entities": qCreatedBy(d10_id+"loft", EntityType.BODY),
                "oppositeDirection":false,
                "transformType" : TransformType.ROTATION,
                "transformAxis": qCreatedBy(d10_id+"rotate2", EntityType.EDGE),
                "angle": 180*degree,
                "makeCopy": true
            });    
            
            opBoolean(context, d10_id + "faceBoolean", {
                    "tools" : qUnion([qCreatedBy(d10_id+"loft", EntityType.BODY),
                                    qCreatedBy(d10_id+"copy1", EntityType.BODY)]),
                    "operationType" : BooleanOperationType.INTERSECTION
                });
            
        }
        if(definition.DiceType == DiceType.d12)
        {
            const d12_id = id+"d12";
            const area = definition.size^2;
            //CONVERT TO: IF D12_Type is reg{}
            if(definition.D12_Type == D12Type.reg){
               
                const d12_side = (7.5/16)*(definition.size);
                const pyr_side = (4.236068)*d12_side;
                const pyr_height = sin(58.282526*degree)*pyr_side;
                const pyr_magnitude = cos(58.282526*degree)*pyr_side;
                const tip_z = 1.376382*d12_side;
                const z_offset = (pyr_height-tip_z)/2;
                
                //pyramid points
                const tip1 = location + (tip_z+z_offset)*vector(0,0,1);
                const base1 = [
                    location+(z_offset*vector(0,0,-1))+(pyr_magnitude*vector(1,0,0)),
                    location+(z_offset*vector(0,0,-1))+(pyr_magnitude*vector(cos(72*degree),sin(72*degree),0)),
                    location+(z_offset*vector(0,0,-1))+(pyr_magnitude*vector(cos(144*degree),sin(144*degree),0)),
                    location+(z_offset*vector(0,0,-1))+(pyr_magnitude*vector(cos(216*degree),sin(216*degree),0)),
                    location+(z_offset*vector(0,0,-1))+(pyr_magnitude*vector(cos(288*degree),sin(288*degree),0)),
                    ];
                
                //Pyramid1
    
                planeCalc(context, d12_id+"pyr1", base1, "base", area);
    
                var sketch1 = newSketch(context, d12_id + "sketch"+"1", {
                                "sketchPlane" : qCreatedBy(d12_id+"pyr1"+"base", EntityType.FACE)
                    });
                skRegularPolygon(sketch1, "polygon1", {
                        "center" : vector(location[0],-location[1]),
                        "firstVertex" : vector(location[0]+pyr_magnitude,-location[1]),
                        "sides" : 5
                });
                skSolve(sketch1);
                opPoint(context, d12_id+"tip",{
                    "point" : tip1});
                opLoft(context, d12_id+"loft", {
                    "profileSubqueries" : [
                        qCreatedBy(d12_id+"sketch"+"1", EntityType.FACE), 
                        qCreatedBy(d12_id+"tip", EntityType.VERTEX)] 
                });
                var sketchPlaneRot = opPlane(context, d12_id+"rotate", {
                "plane": plane(location, vector(0, 0, 1), vector(0, 1, 0))
                });
                var rotationPlane = newSketch(context, d12_id + "rotate2", {
                                "sketchPlane" : qCreatedBy(d12_id+"rotate", EntityType.FACE)
                    });
                var rotateLine = skLineSegment(rotationPlane, "line1", {
                        "start" : vector(location[0],location[1]),
                        "end" : vector(location[0]+sin(36*degree)*inch, location[1]+cos(36*degree)*inch)
                });
                skSolve(rotationPlane);
                transform(context, d12_id+"copy1",{
                    "entities": qCreatedBy(d12_id+"loft", EntityType.BODY),
                    "oppositeDirection":false,
                    "transformType" : TransformType.ROTATION,
                    "transformAxis": qCreatedBy(d12_id+"rotate2", EntityType.EDGE),
                    "angle": 180*degree,
                    "makeCopy": true
                });
                
                //INTERSECTION OF BODIES
                opBoolean(context, d12_id + "faceBoolean", {
                        "tools" : qUnion([qCreatedBy(d12_id+"loft", EntityType.BODY),
                                        qCreatedBy(d12_id+"copy1", EntityType.BODY)]),
                        "operationType" : BooleanOperationType.INTERSECTION
                    });
                //END CONVERT TO: if D12_Type is reg
            }
            
            if(definition.D12_Type == D12Type.rhomboid){
                const rhomb_id = d12_id+"rhomboid";
                const rhomb_dimension = (9.2/(18*sqrt(2)))*definition.size;
                print(rhomb_dimension);
               //Pyramids (6): points at (+-2,0,0) , (0,+-2,0) , (0,0,+-2)
                const tips = [
                    vector(0,0,2)*rhomb_dimension+location,
                    vector(0,0,-2)*rhomb_dimension+location,
                    vector(2,0,0)*rhomb_dimension+location,
                    vector(-2,0,0)*rhomb_dimension+location,
                    vector(0,2,0)*rhomb_dimension+location,
                    vector(0,-2,0)*rhomb_dimension+location,                    
                    ];
                const z_pos = [(vector(1,1,1)*rhomb_dimension)+location, 
                               (vector(1,-1,1)*rhomb_dimension)+location,
                               (vector(-1,-1,1)*rhomb_dimension)+location,
                               (vector(-1,1,1)*rhomb_dimension)+location];
                const z_neg = [(vector(1,1,-1)*rhomb_dimension)+location, 
                               (vector(1,-1,-1)*rhomb_dimension)+location,
                               (vector(-1,-1,-1)*rhomb_dimension)+location,
                               (vector(-1,1,-1)*rhomb_dimension)+location];
                const x_pos = [(vector(1,1,1)*rhomb_dimension)+location, 
                               (vector(1,-1,1)*rhomb_dimension)+location,
                               (vector(1,-1,-1)*rhomb_dimension)+location,
                               (vector(1,1,-1)*rhomb_dimension)+location];
                const x_neg = [(vector(-1,1,1)*rhomb_dimension)+location, 
                               (vector(-1,-1,1)*rhomb_dimension)+location,
                               (vector(-1,-1,-1)*rhomb_dimension)+location,
                               (vector(-1,1,-1)*rhomb_dimension)+location];
                const y_pos = [(vector(1,1,1)*rhomb_dimension)+location, 
                               (vector(1,1,-1)*rhomb_dimension)+location,
                               (vector(-1,1,-1)*rhomb_dimension)+location,
                               (vector(-1,1,1)*rhomb_dimension)+location];
                const y_neg = [(vector(1,-1,1)*rhomb_dimension)+location, 
                               (vector(1,-1,-1)*rhomb_dimension)+location,
                               (vector(-1,-1,-1)*rhomb_dimension)+location,
                               (vector(-1,-1,1)*rhomb_dimension)+location];
               //Planes
                planeCalc(context, rhomb_id+"plane", z_pos, "z_pos", area);
                planeCalc(context, rhomb_id+"plane", z_neg, "z_neg", area);
                planeCalc(context, rhomb_id+"plane", x_pos, "x_pos", area);
                planeCalc(context, rhomb_id+"plane", x_neg, "x_neg", area);
                planeCalc(context, rhomb_id+"plane", y_pos, "y_pos", area);
                planeCalc(context, rhomb_id+"plane", y_neg, "y_neg", area);
                
               //Square and Z_Positive 
                var sketch_z_pos = newSketch(context, rhomb_id + "sketch_z_pos", {
                                "sketchPlane" : qCreatedBy(rhomb_id+"plane"+"z_pos", EntityType.FACE)
                    });
                skRegularPolygon(sketch_z_pos, "z_pos_base", {
                        "center" : vector(location[0],location[1]),
                        "firstVertex" : vector(location[0]+rhomb_dimension,(location[1]+rhomb_dimension)),
                        "sides" : 4
                });
                skSolve(sketch_z_pos);
                opExtrude(context, rhomb_id + "extrude_square", {
                        "entities" : qCreatedBy(rhomb_id+"sketch_z_pos", EntityType.FACE),
                        "direction" : vector(0,0,-1),
                        "endBound" : BoundingType.BLIND,
                        "endDepth" : (2*rhomb_dimension)
                });
                opPoint(context, rhomb_id+"tip_z_pos",{
                    "point" : tips[0]});
                
                //Z_neg
                var sketch_z_neg = newSketch(context, rhomb_id + "sketch_z_neg", {
                                "sketchPlane" : qCreatedBy(rhomb_id+"plane"+"z_neg", EntityType.FACE)
                    });
                skRegularPolygon(sketch_z_neg, "z_neg_base", {
                        "center" : vector(location[0],location[1]),
                        "firstVertex" : vector(location[0]+rhomb_dimension,(location[1]+rhomb_dimension)),
                        "sides" : 4
                });
                skSolve(sketch_z_neg);
                opPoint(context, rhomb_id+"tip_z_neg",{
                    "point" : tips[1]});
                    
                //X_POS
                var sketch_x_pos = newSketch(context, rhomb_id + "sketch_x_pos", {
                                "sketchPlane" : qCreatedBy(rhomb_id+"plane"+"x_pos", EntityType.FACE)
                    });
                skRegularPolygon(sketch_x_pos, "x_pos_base", {
                        "center" : vector(-location[1],location[2]),
                        "firstVertex" : vector(-location[1]+rhomb_dimension,(location[2]+rhomb_dimension)),
                        "sides" : 4
                });
                skSolve(sketch_x_pos);
                opPoint(context, rhomb_id+"tip_x_pos",{
                    "point" : tips[2]});
                //X_NEG
                var sketch_x_neg = newSketch(context, rhomb_id + "sketch_x_neg", {
                                "sketchPlane" : qCreatedBy(rhomb_id+"plane"+"x_neg", EntityType.FACE)
                    });
                skRegularPolygon(sketch_x_neg, "x_neg_base", {
                        "center" : vector(-location[1],location[2]),
                        "firstVertex" : vector(-location[1]+rhomb_dimension,(location[2]+rhomb_dimension)),
                        "sides" : 4
                });
                skSolve(sketch_x_neg);
                opPoint(context, rhomb_id+"tip_x_neg",{
                    "point" : tips[3]});
                    
                //Y_POS
                var sketch_y_pos = newSketch(context, rhomb_id + "sketch_y_pos", {
                                "sketchPlane" : qCreatedBy(rhomb_id+"plane"+"y_pos", EntityType.FACE)
                    });
                skRegularPolygon(sketch_y_pos, "y_pos_base", {
                        "center" : vector(location[0],location[2]),
                        "firstVertex" : vector(location[0]+rhomb_dimension,(location[2]+rhomb_dimension)),
                        "sides" : 4
                });
                skSolve(sketch_y_pos);
                opPoint(context, rhomb_id+"tip_y_pos",{
                    "point" : tips[4]});
                //Y_NEG
                var sketch_y_neg = newSketch(context, rhomb_id + "sketch_y_neg", {
                                "sketchPlane" : qCreatedBy(rhomb_id+"plane"+"y_neg", EntityType.FACE)
                    });
                skRegularPolygon(sketch_y_neg, "y_neg_base", {
                        "center" : vector(location[0],location[2]),
                        "firstVertex" : vector(location[0]+rhomb_dimension,(location[2]+rhomb_dimension)),
                        "sides" : 4
                });
                skSolve(sketch_y_neg);
                opPoint(context, rhomb_id+"tip_y_neg",{
                    "point" : tips[5]});
                
                //LOFTS
                opLoft(context, rhomb_id+"loft" +"z_pos", {
                    "profileSubqueries" : [
                        qCreatedBy(rhomb_id+"sketch_z_pos", EntityType.FACE), 
                        qCreatedBy(rhomb_id+"tip_z_pos", EntityType.VERTEX)] 
                });   
                opLoft(context, rhomb_id+"loft"+"z_neg", {
                    "profileSubqueries" : [
                        qCreatedBy(rhomb_id+"sketch_z_neg", EntityType.FACE), 
                        qCreatedBy(rhomb_id+"tip_z_neg", EntityType.VERTEX)] 
                });
                opLoft(context, rhomb_id+"loft"+"x_pos", {
                    "profileSubqueries" : [
                        qCreatedBy(rhomb_id+"sketch_x_pos", EntityType.FACE), 
                        qCreatedBy(rhomb_id+"tip_x_pos", EntityType.VERTEX)] 
                });
                opLoft(context, rhomb_id+"loft"+"x_neg", {
                    "profileSubqueries" : [
                        qCreatedBy(rhomb_id+"sketch_x_neg", EntityType.FACE), 
                        qCreatedBy(rhomb_id+"tip_x_neg", EntityType.VERTEX)] 
                });
                opLoft(context, rhomb_id+"loft"+"y_pos", {
                    "profileSubqueries" : [
                        qCreatedBy(rhomb_id+"sketch_y_pos", EntityType.FACE), 
                        qCreatedBy(rhomb_id+"tip_y_pos", EntityType.VERTEX)] 
                });
                opLoft(context, rhomb_id+"loft"+"y_neg", {
                    "profileSubqueries" : [
                        qCreatedBy(rhomb_id+"sketch_y_neg", EntityType.FACE), 
                        qCreatedBy(rhomb_id+"tip_y_neg", EntityType.VERTEX)] 
                });
               //Boolean combine
               opBoolean(context, rhomb_id + "boolean", {
                       "tools" : qUnion([qCreatedBy(rhomb_id+"loft", EntityType.BODY),
                                        qCreatedBy(rhomb_id+"extrude_square", EntityType.BODY)]),
                       "operationType" : BooleanOperationType.UNION
               });
            }
            
        }
        if(definition.DiceType == DiceType.d20)
        {
            const d20_id = id+"d20";
            const d20_side = (13/16)*definition.size;
            const mag_20 = 0.850651*d20_side;
            const tipz_d20 = 0.525731*d20_side;
            const middz_d20 = 3.603415*d20_side;
            const middle = 0.850651*d20_side;
            const offsetz = middle/2;
            const area = definition.size^2;
            
            const tip_20 = location+((tipz_d20+offsetz)*vector(0,0,1));
            const tip_mid = location+((middz_d20+offsetz)*vector(0,0,1));
            const tip_mag = mag_20*(tipz_d20*2+middle)/tipz_d20;
            const base20 = [
                location+((tipz_d20+offsetz)*vector(0,0,-1))+(tip_mag*vector(1,0,0)),
                location+((tipz_d20+offsetz)*vector(0,0,-1))+(tip_mag*vector(cos(72*degree),sin(72*degree),0)),
                location+((tipz_d20+offsetz)*vector(0,0,-1))+(tip_mag*vector(cos(144*degree),sin(144*degree),0)),
                location+((tipz_d20+offsetz)*vector(0,0,-1))+(tip_mag*vector(cos(216*degree),sin(216*degree),0)),
                location+((tipz_d20+offsetz)*vector(0,0,-1))+(tip_mag*vector(cos(288*degree),sin(288*degree),0)),
                ];
                
            const mag_inc =  mag_20*(middz_d20+tipz_d20+middle)/middz_d20;
            const midbase_20 = [
                base20[0]+((mag_inc)*vector(1,0,0)),
                base20[1]+((mag_inc)*vector(cos(72*degree),sin(72*degree),0)),
                base20[2]+((mag_inc)*vector(cos(144*degree),sin(144*degree),0)),
                base20[3]+((mag_inc)*vector(cos(216*degree),sin(216*degree),0)),
                base20[4]+((mag_inc)*vector(cos(288*degree),sin(288*degree),0))
            ];
            planeCalc(context, d20_id+"pyr1", base20, "base", area);
            planeCalc(context, d20_id+"pyr2", midbase_20, "base2", area);
            var sketch1 = newSketch(context, d20_id + "sketch1", {
                            "sketchPlane" : qCreatedBy(d20_id+"pyr1"+"base", EntityType.FACE)
                });
            skRegularPolygon(sketch1, "polygon1", {
                    "center" : vector(location[0],-location[1]),
                    "firstVertex" : vector(location[0]+tip_mag,-location[1]),
                    "sides" : 5
            });
            skSolve(sketch1);
            var sketch2 = newSketch(context, d20_id + "sket2", {
                            "sketchPlane" : qCreatedBy(d20_id+"pyr2"+"base2", EntityType.FACE)
            });
            skRegularPolygon(sketch2, "poly2", {
                    "center" : vector(location[0],-location[1]),
                    "firstVertex" : vector(mag_inc+location[0],-location[1]),
                    "sides" : 5
            });
            skSolve(sketch2);
            opPoint(context, d20_id+"tip",{
                "point" : tip_20});
            opPoint(context, d20_id+"tip2",{
                "point" : tip_mid});
            
            opLoft(context, d20_id+"loft1", {
                "profileSubqueries" : [
                    qCreatedBy(d20_id+"sketch1", EntityType.FACE), 
                    qCreatedBy(d20_id+"tip", EntityType.VERTEX)] 
            });
            opLoft(context, d20_id+"loft2", {
                "profileSubqueries" : [
                    qCreatedBy(d20_id+"sket2", EntityType.FACE), 
                    qCreatedBy(d20_id+"tip2", EntityType.VERTEX)] 
            });
            //ROTATION
            var sketchPlaneRot = opPlane(context, d20_id+"rotate", {
            "plane": plane(location, vector(0, 0, 1), vector(0, 1, 0))
            });
            var rotationPlane = newSketch(context, d20_id + "rotate2", {
                            "sketchPlane" : qCreatedBy(d20_id+"rotate", EntityType.FACE)
                });
            var rotateLine = skLineSegment(rotationPlane, "line1", {
                    "start" : vector(location[0],location[1]),
                    "end" : vector(location[0]+sin(36*degree)*inch,location[1]+cos(36*degree)*inch)
            });
            skSolve(rotationPlane);
            transform(context, d20_id+"copy1",{
                "entities": qUnion([qCreatedBy(d20_id+"loft1", EntityType.BODY), qCreatedBy(d20_id+"loft2", EntityType.BODY)]),
                "oppositeDirection":false,
                "transformType" : TransformType.ROTATION,
                "transformAxis": qCreatedBy(d20_id+"rotate2", EntityType.EDGE),
                "angle": 180*degree,
                "makeCopy": true
            });
            
            opBoolean(context, d20_id + "faceBoolean", {
                    "tools" : qUnion([qCreatedBy(d20_id+"copy1", EntityType.BODY),
                        qUnion([qCreatedBy(d20_id+"loft1", EntityType.BODY), qCreatedBy(d20_id+"loft2", EntityType.BODY)])]),
                    "operationType" : BooleanOperationType.INTERSECTION
                });
            
        }
    }
    );

function planeCalc(context is Context, id is Id, face is array, name is string, area)
{
    //Calculate Plane through 3+ points
    var total = vector(0,0,0)*millimeter;
    var numPoints = size(face);
    
    for (var item in face)
    {
        //println(item);
        total += item;
    }
    //println(total);
    var a = face[0];
    var b = face[1];
    var c = face[2];
    var _origin = total/numPoints; //calculates center point of face
    var _normal = cross((c-a), (b-a)); //uses 3 of the points on a face to make the plane
    opPlane(context, id + name, {
            "plane" : plane(_origin, _normal),
            "width" : sqrt(area),
            "height" : sqrt(area)
    });
}

export function editDiceLogic(context is Context, id is Id, oldDefinition is map, definition is map, isCreating is boolean, specifiedParameters is map, hiddenBodies is Query) returns map
{
    // isCreating is required in the function definition for edit logic to work when editing an existing feature

    return definition;
}

export enum DiceType
{
        annotation {"Name": "D4"}
        d4,
        annotation {"Name": "D6"}
        d6,
        annotation {"Name": "D8"}
        d8,
        annotation {"Name": "D10"}
        d10,
        annotation {"Name": "D12"}
        d12,
        annotation {"Name": "D20"}
        d20
}
export enum D4Type
{
        annotation {"Name": "Regular"}
        reg,
        annotation {"Name": "Crystal"}
        crystal,
        annotation {"Name": "Shard"}
        shard,

}
export enum D12Type
{
        annotation {"Name": "Regular"}
        reg,
        annotation {"Name": "Rhomboid"}
        rhomboid,
}
const SIZE_BOUNDS = 
{
        (meter) : [1e-5, 0.00002, 0.25],
        (centimeter) : 0.002,
        (millimeter) : 0.2,
        (inch) : 0.787402
} as LengthBoundSpec;
