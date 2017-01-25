﻿namespace Repo

open QuickGraph
open System.Collections.Generic

type internal ModelElementAttributes = 
    { 
        id : Id;
        name : string;
        potency : int;
        level : int 
    }

/// Attribute value is a reference to an instance of attribute type, which shall be present in a model.
/// For pragmatic reasons instances of basic types (int and string) are not proper nodes in a model, but merely string and int values.
and internal AttributeValue =
    | None
    | Ref of VertexLabel
    | Int of int
    | String of string

and internal AttributeAttributes =
    {
        value : AttributeValue
    }

and internal NodeKind =
    | Node
    | Attribute of AttributeAttributes

and internal VertexLabel =
    ModelElementAttributes * NodeKind

type internal AssociationAttributes =
    {
        sourceName : string;
        sourceMin : int;
        sourceMax : int;
        targetName : string;
        targetMin : int;
        targetMax : int;
    }

type internal EdgeKind = 
    | Generalization
    | Attribute
    | Type
    | Value
    | Association of AssociationAttributes

type internal EdgeLabel = ModelElementAttributes * EdgeKind

type internal ModelElementLabel = 
    | Vertex of VertexLabel
    | Edge of EdgeLabel

type internal RepoRepresentation = BidirectionalGraph<VertexLabel, TaggedEdge<VertexLabel, EdgeLabel>> * Dictionary<ModelElementLabel, ModelElementLabel>

module internal GraphMetametamodel =

    let private newId () = System.Guid.NewGuid().ToString()

    let private createGeneralizationLabel () =
        let modelElementAttributes = { id = newId(); name = "Generalization"; potency = 0; level = 0 }
        (modelElementAttributes, Generalization)

    let private createAttributeLabel () =
        let modelElementAttributes = { id = newId(); name = "Attribute"; potency = 0; level = 0 }
        (modelElementAttributes, Attribute)

    let private createTypeLabel () =
        let modelElementAttributes = { id = newId(); name = "Type"; potency = 1; level = 0 }
        (modelElementAttributes, Type)

    let private createValueLabel () =
        let modelElementAttributes = { id = newId(); name = "Value"; potency = 0; level = 0 }
        (modelElementAttributes, Value)

    let private createAssociationLabel targetName targetMin targetMax =
        let modelElementAttributes = { id = newId(); name = "Association"; potency = -1; level = 0 }
        let associationAttributes = { sourceName = "Source"; sourceMin = 1; sourceMax = 1; targetName = targetName; targetMin = targetMin; targetMax = targetMax }
        (modelElementAttributes, Association associationAttributes)

    let createM0Model () =
        let repo : RepoRepresentation = (new BidirectionalGraph<_, _> true, new Dictionary<_, _>())
        let repoGraph, classes = repo

        let createEdge label source target = 
            let edge = new TaggedEdge<_, _>(source, target, label)
            repoGraph.AddEdge edge |> ignore
            label

        let createNode name potency = 
            let vertex = { id = newId (); name = name; potency = potency; level = 0 }
            repoGraph.AddVertex (vertex, Node) |> ignore
            vertex, Node

        let (~+) name = 
            createNode name -1

        let (~-) name = 
            createNode name 0

        let (---|>) source target = createEdge (createGeneralizationLabel ()) source target |> ignore

        let (--+-->) source target = createEdge (createAttributeLabel ()) source target |> ignore

        let (--*-->) source target = createEdge (createTypeLabel ()) source target |> ignore

        let createAssociation source target targetName targetMin targetMax = createEdge (createAssociationLabel targetName targetMin targetMax) source target

        let (--@-->) source target = classes.Add(source, target)

        let modelElement = -"ModelElement"
        let node = +"Node"
        let attribute = +"Attribute"
        let relationship = -"Relationship"
        let generalization = +"Generalization"
        let association = +"Association"

        let stringType = -"String"
        let intType = -"Int"

        let classAssociation = createAssociation modelElement modelElement "class" 1 1
        createAssociation modelElement attribute "attributes" 0 -1 |> ignore
        createAssociation attribute node "type" 1 1 |> ignore

        let valueAssociation = createAssociation attribute node "value" 0 1
        Edge valueAssociation --@--> Vertex association

        createAssociation relationship modelElement "source" 1 1 |> ignore
        createAssociation relationship modelElement "target" 1 1 |> ignore

        let createAttribute node name type' value =
            let attributeAttributes = { value = value }
            let modelElementAttributes = { id = newId(); name = name; potency = 0; level = 0 }
            let attributeLabel = modelElementAttributes, NodeKind.Attribute attributeAttributes
            repoGraph.AddVertex attributeLabel |> ignore

            Vertex attributeLabel --@--> Vertex attribute
            match value with 
            | Ref v ->
                let valueLink = createEdge (createValueLabel ()) attributeLabel v 
                Edge valueLink --@--> Edge valueAssociation
            | _ -> ()

            node --+--> attributeLabel
            attributeLabel --*--> type'

        createAttribute modelElement "Name" stringType (String "ModelElement")
        createAttribute modelElement "Potency" intType (Int -1)
        createAttribute modelElement "Level" intType (Int 0)

        createAttribute association "MinSource" intType None
        createAttribute association "MaxSource" intType None
        createAttribute association "SourceName" stringType None
        createAttribute association "MinTarget" intType None
        createAttribute association "MaxTarget" intType None
        createAttribute association "TargetName" stringType None

        Vertex stringType --@--> Vertex node
        Vertex intType --@--> Vertex node

        node ---|> modelElement
        attribute ---|> modelElement
        relationship ---|> modelElement
        generalization ---|> relationship
        association ---|> relationship

        (repoGraph, classes)
