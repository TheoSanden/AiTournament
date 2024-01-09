using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphs
{
    public class Link : ILink
    {
        private INode   m_source;
        private INode   m_target;

        #region Properties

        public INode Source => m_source;

        public INode Target => m_target;

        #endregion

        public Link(INode source, INode target)
        {
            m_source = source;
            m_target = target;
        }
    }
}