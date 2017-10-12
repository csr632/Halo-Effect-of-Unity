# 项目内容
利用Unity粒子系统制作光环

# 粒子系统概述
根据[官方文档](https://docs.unity3d.com/Manual/PartSysWhatIs.html)的解释，粒子系统管理**大量既小又简单**的图片或网格（mesh）一起运动，组成一个整体的效果。比如利用粒子系统可以制作出烟雾效果，每一个粒子是一个微小的烟云的图片，成千上万个这样的粒子就组成了一整块烟雾的效果，用普通的方法就很难做出烟雾的效果。

# 粒子系统的重要属性
* 生命周期lifetime：每一个粒子都有一个生命周期，表示粒子被发射（emit）出来以后能存活多长时间。你可以在Inspector中设置Start lifetime来设置粒子的生命周期：
![Inspector中设置lifetime](http://upload-images.jianshu.io/upload_images/4888929-9d0ba690577dcba5.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
> 之所以叫start是因为你设置的只是粒子默认的生命周期，在运行过程中lifetime还有可能被脚本改变。

* 发射速率emission rate：每秒钟发射多少个粒子。你可以在Inspector中找到Emission模块的rate over time中设置发射速率。
> 实际发射粒子的时机有一定的随机性，不一定是间隔均匀地发射。

以上两个属性描述的是整个粒子系统的状态。我们还可以控制单个粒子的样式和行为。

* 粒子个体的属性：速度矢量、颜色、大小、朝向等。有一些属性除了可以设置常量值以外，还可以设置成随着时间变化的值，或者在一定范围内随机的值。你只需要点击输入框右边的下拉三角按钮，就可以改变设置方式。

> 由于整个粒子系统有很多的属性可以自定义，因此Unity将它划分成了很多个模块（Particle System modules），方便查找。这是[粒子系统模块的参考文档](https://docs.unity3d.com/Manual/ParticleSystemModules.html)。

# 项目概述
这个项目制作一个简单的粒子光环，光环中的粒子缓慢移动，看起来像太阳系的小行星带。效果尽量类似[http://i-remember.fr/en](http://i-remember.fr/en)。要实现这个效果，需要使用脚本来控制每一个粒子。

# 效果图
![](http://upload-images.jianshu.io/upload_images/4888929-9a36d664b32aaa0c.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

# 在自己的电脑上运行！
从[我的github](https://github.com/csr632/Halo-Effect-of-Unity/tree/master)下载项目资源，将所有文件放进你的项目的Assets文件夹（如果有重复则覆盖），然后在Unity3D中双击“hw9”，就可以运行了！


# 代码
ParticleStatus 用于记录某个粒子的状态：
```
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
```
ParticleHalo控制一个含有粒子系统的对象，生成一个光环：
```
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
        particleSys.GetParticles(particleArr);  // 将发射的粒子存在particleArr数组中
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
            // 将所有粒子根据下标i，给5个不同的速度，分别是rotateSpeed的1/5、2/5……5/5
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

            particleArr[i].color = colorGradient.Evaluate(StatusArr[i].angle / 360.0f); // 根据粒子的angle，给粒子赋予不同的透明度（颜色），使某一些角度上的粒子暗一些
        }

        particleSys.SetParticles(particleArr, particleArr.Length);
    }
}
```
NormalDistribution 用于产生正态分布的随机数，原理是*[Marsaglia polar method](https://en.wikipedia.org/wiki/Marsaglia_polar_method)*：
```
using System;

public class NormalDistribution {
	// use Marsaglia polar method to generate normal distribution
    private bool _hasDeviate;
    private double _storedDeviate;
    private readonly Random _random;

    public NormalDistribution(Random random = null)
    {
        _random = random ?? new Random();
    }

    public double NextGaussian(double mu = 0, double sigma = 1)
    {
        if (sigma <= 0)
            throw new ArgumentOutOfRangeException("sigma", "Must be greater than zero.");

        if (_hasDeviate)
        {
            _hasDeviate = false;
            return _storedDeviate*sigma + mu;
        }

        double v1, v2, rSquared;
        do
        {
            // two random values between -1.0 and 1.0
            v1 = 2*_random.NextDouble() - 1;
            v2 = 2*_random.NextDouble() - 1;
            rSquared = v1*v1 + v2*v2;
            // ensure within the unit circle
        } while (rSquared >= 1 || rSquared == 0);

        // calculate polar tranformation for each deviate
        var polar = Math.Sqrt(-2*Math.Log(rSquared)/rSquared);
        // store first deviate
        _storedDeviate = v2*polar;
        _hasDeviate = true;
        // return second deviate
        return v1*polar*sigma + mu;
    }
}

```
> 代码意义已经在注释中解释

****
# 两层光环
为了做出两层光环，只需要将同一份脚本挂载在**两个**具有Particle System的对象上，然后在Inspector中调整一下参数，就可以实现内层逆时针、外层顺时针、内层快、外层慢、内层稠密、外层稀疏的效果了。
![外层参数](http://upload-images.jianshu.io/upload_images/4888929-39933722149d7e50.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


![内层参数](http://upload-images.jianshu.io/upload_images/4888929-f409546a6c5dbb2b.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
