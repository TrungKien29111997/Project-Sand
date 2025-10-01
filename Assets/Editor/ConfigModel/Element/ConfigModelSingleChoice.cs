using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace TrungKien.Window.Element
{
    public class ConfigModelSingleChoice : ConfigModelNode
    {
        public override void Init(Vector2 pos)
        {
            base.Init(pos);
            choices.Add("New Choice");
        }
        public override void Draw()
        {
            base.Draw();
            foreach (string choice in choices)
            {
                Port port = this.CreatePort(choice, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
                outputContainer.Add(port);
            }
            RefreshExpandedState();
        }
    }
}