// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    /// <summary>
    /// Add this component to a GameObject to make it selectable by the sample Lexicon actions.
    /// This requires the GameObject or one of its children to have a collider.
    /// The Selection highlighting here is just a sample, it requires a Standard shader to work.
    /// </summary>
    public class LexiconSelectable : MonoBehaviour
    {
        private bool selected;

        private Renderer _renderer;
        private Material originalMaterial;
        private Material selectedMaterial;

        private Texture originalTexture;
        private Color originalColor;
        private bool originalEnabled;

        public void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();

            originalMaterial = _renderer.material;
            selectedMaterial = new Material(originalMaterial);

            selectedMaterial.SetTexture("_EmissionMap", null);
            selectedMaterial.SetColor("_EmissionColor", new Color(0.3f, 0.3f, 0.3f));
            selectedMaterial.EnableKeyword("_EMISSION");
        }

        public void Select()
        {
            if (!selected)
            {
                _renderer.material = selectedMaterial;
                selected = true;
            }
        }

        public void Deselect()
        {
            if (selected)
            {
                _renderer.material = originalMaterial;
                selected = false;
            }
        }

        public void UpdateMaterials()
        {
            originalMaterial.color = _renderer.material.color;
            selectedMaterial.color = _renderer.material.color;
        }
    }
}
