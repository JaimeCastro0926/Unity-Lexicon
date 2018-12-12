// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

// Shorthand for the auto-generated strings belonging to this intent.
using Strings = Mixspace.Lexicon.Actions.CreateModelStrings;

namespace Mixspace.Lexicon.Actions
{
    public class CreateModelAction : LexiconAction
    {
        public Shader shader;

        public override bool Process(LexiconRuntimeResult runtimeResult)
        {
            // There may be multiple models, break the phrase up by model entity.
            // E.g. "Create a black king here and a white queen here."
            foreach (LexiconEntityMatch modelMatch in runtimeResult.GetEntityMatches(Strings.ChessPiece))
            {
                GameObject modelPrefab = modelMatch.EntityValue.GetBinding<GameObject>();
//                Material material = new Material(shader);

                // We expect the color entity (if present) to come before the model entity.
                LexiconEntityMatch colorMatch = runtimeResult.GetEntityBefore(Strings.Color, modelMatch);
             //   if (colorMatch != null)
           //     {
                //    material.color = colorMatch.EntityValue.GetBinding<Color>();
             //   }

                // Create the model.
                GameObject model = Instantiate(modelPrefab);
                model.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
             //   model.GetComponentInChildren<Renderer>().material = material;
                model.AddComponent<LexiconSelectable>();

                // We expect the position (if present) to come after the model entity.
                LexiconEntityMatch positionMatch = runtimeResult.GetEntityAfter(Strings.Position, modelMatch);
                if (positionMatch != null && positionMatch.FocusPosition != null)
                {
                    // Use the focus position of the position entity if present.
                    model.transform.position = positionMatch.FocusPosition.Position;
                }
                else if (modelMatch.FocusPosition != null)
                {
                    // Otherwise use the focus position of the model entity.
                    model.transform.position = modelMatch.FocusPosition.Position;
                }
                else
                {
                    // As a fall back we can place the object in front of the camera.
                    Camera mainCamera = Camera.main;
                    if (mainCamera != null)
                    {
                        model.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 2.0f;
                    }
                }
            }

            // We've consumed this intent, return true to prevent other handlers from firing.
            return true;
        }
    }
}
