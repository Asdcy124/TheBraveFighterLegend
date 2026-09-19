public interface I_状态机
{
    public void 切换状态(int 当前状态, int 下一个状态);
    public int 获取下一个状态(int 当前状态);
    public void 状态机_PhysicsProcess(int 当前状态, double delta);
}