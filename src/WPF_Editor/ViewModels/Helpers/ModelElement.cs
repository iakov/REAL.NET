﻿using System.Collections.Generic;
using Repo;

namespace WPF_Editor.ViewModels.Helpers
{
    public abstract class ModelElement : IElement
    {
        public IElement Element { get; }
        public string Name
        {
            get => Element.Name;
            set => Element.Name = value;
        }

        public IElement Class => Element.Class;

        public IEnumerable<IAttribute> Attributes => Element.Attributes;

        public bool IsAbstract => Element.IsAbstract;

        public Metatype Metatype => Element.Metatype;

        public Metatype InstanceMetatype => Element.InstanceMetatype;

        public string Shape => Element.Shape;

        protected ModelElement(IElement element)
        {
            Element = element;
        }
        public override string ToString() => Name;
    }
}