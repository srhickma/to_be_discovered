using System.Collections.Generic;
using System.Linq;

public class Floor {

    private List<int> walls = new List<int>();
    private readonly List<NavNode> nodes = new List<NavNode>();
    public int y { get; set; }

    public Floor(int y){
        this.y = y;
    }

    public void setWalls(List<int> walls){
        this.walls = walls;
    }
    
    public List<int> getWalls(){
        return walls;
    }

    public void addNode(NavNode node){
        nodes.Add(node);
    }

    public List<NavNode> getNodes(){
        return nodes;
    }

    public void connectNodes(){
        nodes.Sort((n1, n2) => n1.x.CompareTo(n2.x));
        for(int i = 0; i < nodes.Count - 1; i ++){
            Range joinRange = new Range(nodes[i].x, nodes[i + 1].x);
            if(walls.All(x => !joinRange.contains(x))){
                NavNode.join(nodes[i], nodes[i + 1]);
            }
        }
    }

    public bool noWallsBetween(Range range){
        return walls.All(x => !range.contains(x));
    }
    
}
