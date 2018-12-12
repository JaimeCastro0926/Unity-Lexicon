// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

// Shorthand for the auto-generated strings belonging to this intent.
using Strings = Mixspace.Lexicon.Actions.CreatePrimitiveStrings;

namespace Mixspace.Lexicon.Actions
{
    public class CreatePrimitiveAction : LexiconAction
    {
        public Shader shader;

        public override bool Process(LexiconRuntimeResult runtimeResult)
        {
            // There may be multiple primitives, break the phrase up by primitive entity.
            // E.g. "Create a red cube here and a blue sphere here."
            foreach (LexiconEntityMatch primitiveMatch in runtimeResult.GetEntityMatches(Strings.Primitive))
            {
                PrimitiveType primitiveType = primitiveMatch.EntityValue.GetBinding<PrimitiveType>();
                Material material = new Material(shader);

                // We expect the color entity (if present) to come before the primitive entity.
                LexiconEntityMatch colorMatch = runtimeResult.GetEntityBefore(Strings.Color, primitiveMatch);
                if (colorMatch != null)
                {
                    material.color = colorMatch.EntityValue.GetBinding<Color>();
                }

                // Create the primitive. Use a container to anchor from the bottom.
                GameObject container = new GameObject(primitiveType.ToString());
                GameObject primitive = GameObject.CreatePrimitive(primitiveType);
                primitive.GetComponent<Renderer>().material = material;
                float yOffset = primitive.GetComponent<Renderer>().bounds.extents.y;
                primitive.transform.parent = container.transform;
                primitive.transform.localPosition = new Vector3(0.0f, yOffset, 0.0f);
                container.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                container.AddComponent<LexiconSelectable>();

                // We expect the position (if present) to come after the primitive entity.
                LexiconEntityMatch positionMatch = runtimeResult.GetEntityAfter(Strings.Position, primitiveMatch);
                if (positionMatch != null && positionMatch.FocusPosition != null)
                {
                    // Use the focus position of the position entity if present.
                    container.transform.position = positionMatch.FocusPosition.Position;
                }
                else if (primitiveMatch.FocusPosition != null)
                {
                    // Otherwise use the focus position of the primitive entity.
                    container.transform.position = primitiveMatch.FocusPosition.Position;
                }
                else
                {
                    // As a fall back we can place the object in front of the camera.
                    Camera mainCamera = Camera.main;
                    if (mainCamera != null)
                    {
                        container.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 2.0f; 
                    }
                }
            }

            // We've consumed this intent, return true to prevent other handlers from firing.
            return true;
        }
    }
}
