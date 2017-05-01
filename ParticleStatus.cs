public class ParticleStatus {
	public float radius = 0f, angle = 0f, time = 0f, radiusChange = 0.02f; // 粒子轨道变化范围;  
    public ParticleStatus(float radius, float angle, float time, float radiusChange)  
    {  
        this.radius = radius;   // 半径  
        this.angle = angle;     // 角度
		this.time = time;       // 变化的进度条，用于轨道半径的变化
        radiusChange = this.radiusChange;       // 轨道半径的变化程度
    }  
}
