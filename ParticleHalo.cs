using UnityEngine;

public class ParticleHalo : MonoBehaviour
{

    private ParticleSystem particleSys;  // 粒子系统组件
    private ParticleSystem.Particle[] particleArr;  // 粒子数组  
    private ParticleStatus[] StatusArr; // 记录粒子状态的数组
    public int particleNum = 10000; // 粒子数量
    public float minRadius = 8.0f; // 光环最小半径
    public float maxRadius = 12.0f; // 光环最大半径
    public float maxRadiusChange = 0.02f; // 粒子轨道变化的平均值
    public bool clockwise = true;  // 光环是否顺时针旋转
    public float rotateSpeed = 0.3f;  // 光环旋转速度
    public int speedLevel = 5; // 速度有多少个层次
    private NormalDistribution normalGenerator; // 高斯分布生成器
    public Gradient colorGradient;  // 控制粒子的透明度


    void Start()
    {
        particleSys = GetComponent<ParticleSystem>();
        particleArr = new ParticleSystem.Particle[particleNum];
        StatusArr = new ParticleStatus[particleNum];

        var ma = particleSys.main;  // 通过ma来设置粒子系统的maxParticles
        ma.maxParticles = particleNum;

        particleSys.Emit(particleNum);  // 同时发射particleNum个粒子
        particleSys.GetParticles(particleArr);  // 将发射的例子存在particleArr数组中
        normalGenerator = new NormalDistribution(); // 初始化高斯分布生成器

        // 初始化梯度颜色控制器  
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];
        alphaKeys[0].time = 0.0f; alphaKeys[0].alpha = 1.0f;
        alphaKeys[1].time = 0.4f; alphaKeys[1].alpha = 0.4f;
        alphaKeys[2].time = 0.6f; alphaKeys[2].alpha = 1.0f;
        alphaKeys[3].time = 0.9f; alphaKeys[3].alpha = 0.4f;
        alphaKeys[4].time = 1.0f; alphaKeys[4].alpha = 0.9f;
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].time = 0.0f; colorKeys[0].color = Color.white;
        colorKeys[1].time = 1.0f; colorKeys[1].color = Color.white;
        colorGradient.SetKeys(colorKeys, alphaKeys);

        initParticle();
    }

    void initParticle()
    {
        for (int i = 0; i < particleNum; i++)
        {
            // 普通的随机半径生成
            // float midRadius = (maxRadius + minRadius) / 2;
            // float minRate = Random.Range(1.0f, midRadius / minRadius);
            // float maxRate = Random.Range(midRadius / maxRadius, 1.0f);
            // float radius = Random.Range(minRadius * minRate, maxRadius * maxRate);

            // 使用高斯分布生成半径， 均值为midRadius，标准差为0.7
            float midRadius = (maxRadius + minRadius) / 2;
            float radius = (float)normalGenerator.NextGaussian(midRadius, 0.7);

            float angle = Random.Range(0.0f, 360.0f);
            float theta = angle / 180 * Mathf.PI;
            float time = Random.Range(0.0f, 360.0f);    // 给粒子生成一个随机的初始进度
            float radiusChange = Random.Range(0.0f, maxRadiusChange);   // 随机生成一个轨道变化大小
            StatusArr[i] = new ParticleStatus(radius, angle, time, radiusChange);
            particleArr[i].position = computePos(radius, theta);
        }
        particleSys.SetParticles(particleArr, particleArr.Length);
    }

    Vector3 computePos(float radius, float theta)
    {
        return new Vector3(radius * Mathf.Cos(theta), 0f, radius * Mathf.Sin(theta));
    }

    void Update()
    {
        for (int i = 0; i < particleNum; i++)
        {
            // 将所有例子根据下标i，给5个不同的速度，分别是rotateSpeed的1/5、2/5……5/5
            if (!clockwise)
            {
                StatusArr[i].angle += (i % speedLevel + 1) * (rotateSpeed / speedLevel);
            }
            else
            {
                StatusArr[i].angle -= (i % speedLevel + 1) * (rotateSpeed / speedLevel);
            }

            // angle range guarantee
            StatusArr[i].angle = (360.0f + StatusArr[i].angle) % 360.0f;
            float theta = StatusArr[i].angle / 180 * Mathf.PI;

            StatusArr[i].time += Time.deltaTime;    // 增加粒子的进度
            StatusArr[i].radius += Mathf.PingPong(StatusArr[i].time / maxRadius / maxRadius, StatusArr[i].radiusChange) - StatusArr[i].radiusChange / 2.0f; // 根据粒子的进度，给粒子的半径赋予不同的值，这个值在0与StatusArr[i].radiusChange之间来回摆动

            particleArr[i].position = computePos(StatusArr[i].radius, theta);

            particleArr[i].color = colorGradient.Evaluate(StatusArr[i].angle / 360.0f); // 根据粒子的angle，给粒子赋予不同的透明度（颜色），使某一些角度上的例子暗一些
        }

        particleSys.SetParticles(particleArr, particleArr.Length);
    }
}
