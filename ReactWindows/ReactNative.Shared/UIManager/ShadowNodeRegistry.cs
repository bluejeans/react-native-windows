// Copyright (c) Microsoft Corporation. All rights reserved.
// Portions derived from React Native:
// Copyright (c) 2015-present, Facebook, Inc.
// Licensed under the MIT License.

using Facebook.Yoga;
using System;
using System.Collections.Generic;

namespace ReactNative.UIManager
{
    /// <summary>
    /// Simple container class to keep track of <see cref="ReactShadowNode"/>s
    /// associated with a particular <see cref="UIManagerModule"/> instance.
    /// </summary>
    public class ShadowNodeRegistry
    {
        private readonly object _gate = new object();

        private readonly IDictionary<int, ReactShadowNode> _tagsToCssNodes =
            new Dictionary<int, ReactShadowNode>();

        private readonly IDictionary<int, bool> _rootTags =
            new Dictionary<int, bool>();

        // The RootNodeTags API is called from UI thread instead of the
        // layout thread. Occasionally, we would get an exception related to
        // the enumeration of the Keys collection being disrupted by an
        // AddRootNode operation. This was especially likely in Debug mode, but
        // also could occur during a reload of the application from CodePush.
        // To get around this, we copy the key collection into this list.
        private List<int> _rootNodeTags = new List<int>();

        /// <summary>
        /// The collection of root node tags.
        /// </summary>
        public IReadOnlyList<int> RootNodeTags
        {
            get
            {
                _rootNodeTags.Clear();

                lock (_gate)
                {
                    foreach (var tag in _rootTags.Keys)
                    {
                        _rootNodeTags.Add(tag);
                    }
                }

                return _rootNodeTags;
            }
        }  

        /// <summary>
        /// Add a root shadow node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void AddRootNode(ReactShadowNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var tag = node.ReactTag;
            _tagsToCssNodes[tag] = node;

            lock (_gate)
            {
                _rootTags[tag] = true;
            }
        }

        /// <summary>
        /// Remove a root shadow node.
        /// </summary>
        /// <param name="tag">The tag of the node to remove.</param>
        public void RemoveRootNode(int tag)
        {
            if (!_rootTags.ContainsKey(tag))
            {
                throw new KeyNotFoundException($"View with tag '{tag}' is not registered as a root view.");
            }

            _tagsToCssNodes.Remove(tag);

            lock (_gate)
            {
                _rootTags.Remove(tag);
            }
        }

        /// <summary>
        /// Add a React shadow node.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddNode(ReactShadowNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            _tagsToCssNodes[node.ReactTag] = node;
        }

        /// <summary>
        /// Remove a React shadow node.
        /// </summary>
        /// <param name="tag">The tag of the node to remove.</param>
        public void RemoveNode(int tag)
        {
            if (_rootTags.TryGetValue(tag, out var isRoot) && isRoot)
            {
                throw new KeyNotFoundException($"Trying to remove root node '{tag}' without using RemoveRootNode.");
            }

            _tagsToCssNodes.Remove(tag);
        }

        /// <summary>
        /// Retrieve a React shadow node.
        /// </summary>
        /// <param name="tag">The tag of the node to retrieve.</param>
        /// <returns>The React shadow node.</returns>
        public ReactShadowNode GetNode(int tag)
        {
            if (_tagsToCssNodes.TryGetValue(tag, out var result))
            {
                return result;
            }

            throw new KeyNotFoundException($"Shadow node for tag '{tag}' does not exist.");
        }

        /// <summary>
        /// Checks if a node with the given tag is a root node.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>
        /// <b>true</b> if the node with the given tag is a root node,
        /// <b>false</b> otherwise.
        /// </returns>
        public bool IsRootNode(int tag)
        {
            return _rootTags.ContainsKey(tag);
        }
    }
}
