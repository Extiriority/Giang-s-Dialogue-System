using System.Collections.Generic;

public class GDSNodeErrorData  {
    public GDSErrorData errorData { get; set; } = new GDSErrorData();
    public List<GDSNode> nodes { get; set; } = new List<GDSNode>();
}
