using System;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FeteTimer.Game
{
    /// <summary>
    /// Recursively walks an addon's node tree (children + nested component sub-trees) and invokes a
    /// callback for every text node with its id and current text. All access is unsafe and read-only;
    /// individual node text reads are guarded so a bad node cannot throw out of the walk.
    /// </summary>
    internal static unsafe class NodeWalker
    {
        internal static void WalkTextNodes(AtkUnitBase* addon, Action<uint, string> visit)
        {
            if (addon == null || addon->RootNode == null)
                return;

            Walk(addon->RootNode, 0, visit);
        }

        private static void Walk(AtkResNode* node, int depth, Action<uint, string> visit)
        {
            // Children are reached via ChildNode, then the PrevSiblingNode chain (Dalamud idiom).
            for (var n = node; n != null; n = n->PrevSiblingNode)
            {
                if (depth > 48)
                    return;

                if (n->Type == NodeType.Text)
                {
                    var tn = (AtkTextNode*)n;
                    var text = string.Empty;
                    if (tn->NodeText.StringPtr.HasValue)
                    {
                        try
                        {
                            text = MemoryHelper.ReadSeStringNullTerminated((nint)tn->NodeText.StringPtr.Value).TextValue;
                        }
                        catch
                        {
                            text = string.Empty;
                        }
                    }

                    visit(n->NodeId, text);
                }

                if (n->ChildNode != null)
                    Walk(n->ChildNode, depth + 1, visit);

                // Component nodes carry their own sub-tree under Component->UldManager.
                if ((ushort)n->Type >= 1000)
                {
                    var comp = ((AtkComponentNode*)n)->Component;
                    if (comp != null && comp->UldManager.RootNode != null)
                        Walk(comp->UldManager.RootNode, depth + 1, visit);
                }
            }
        }
    }
}
