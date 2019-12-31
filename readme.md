# Unity_Knit

[![license](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Tencent/InjectFix/blob/master/LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/Tencent/InjectFix/pulls)

This project is made for my first wedding ceremoy. I use Unity to made an implementation of [petros vrellis's knitting](http://artof01.com/vrellis/works/knit.html). The method is inspired by [Eric Heitz's Research Page](https://eheitzresearch.wordpress.com/)

![input](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/input.jpg)
![prediction](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/prediction2.jpg)
![realization](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/realization2.jpg)

![closeup1](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/closeup1.jpg)

![closeup2](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/closeup2.jpg)
![wip](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/wip.jpg)

![sequence](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/sequnce.jpg)
Input parameters:
Input image, Nails number, knitting path number, cavas resolution.

We set the cavas resolution to be 2048. The cavase and fish line are bought from Taobao.
The cavas is 50 cm in diameter and 157 cm in perimeters. The nails is knocked 5 mm each. Fish line is 0.12mm in diameter and 1500m long.

## 中文
### 简介
这项工程是我和我老婆的结婚第一年生日礼物，是用线缝纫出来的我们的双人头像。这项工程使用图形学进行建模，然后使用钉子，画板和鱼线进行图像在现实世界上的绘制。是一次计算机图形学应用在现实世界中的尝试。
![input](https://github.com/sienaiwun/Unity_Knit/blob/master/Images/input.jpg?raw=true)
![prediction](https://github.com/sienaiwun/Unity_Knit/blob/master/Images/prediction2.jpg?raw=true)
![realization](https://github.com/sienaiwun/Unity_Knit/blob/master/Images/realization2.jpg?raw=true)

这项工程的输入参数有头像照片，钉子数目，缝纫线端长度和画布尺寸。
材料是由淘宝购得，分别是50cm直径圆形画板，0.12mm 黑色鱼线1500m，和大头钉若干。

### 预备画板
第一步是将钉子平铺在圆形画板的直径上。我们直接在淘宝上买的50cm直接圆形画板。我们用5mm每个的密度钉钉子，总共有315个钉子。

![closeup](https://github.com/sienaiwun/Unity_Knit/blob/master/Images/closeup1.jpg?raw=true)

![closeup2](https://github.com/sienaiwun/Unity_Knit/blob/master/Images/closeup2.jpg?raw=true)

### 计算连线路径
我们设定总共作绣的线段数目为4千个。这4千个钉子编号分布在[0,314], 对应着画板圆圈上的315个钉子。我们连线的目的就是希望用4千个表示钉子到钉子之间连线的线段来模拟输入图像。
我们使用随机优化的思想进行计算缝线路径。既随机扰动一个选段结点，如果该扰动使生成图片和输入图片的误差减小，我们就保留这个扰动，否则丢弃这个扰动，继续迭代。
伪代码：

``` cpp
while(true)
{
    // random mutation
    node = rand()%NumberOfNodes;
    current_nail = thread_path[node];
    candidate_nail = rand()%NumberOfNodes;
    thread_path[node] = candidate_nail
    
    // if the error is small
    float error = computeError();
    if(error < current_error)
        current_error = error;
    else
        thread_path[node] = current_nail;
}
``` 
![sequence](https://github.com/sienaiwun/Unity_Knit/blob/master/Images/sequnce.jpg?raw=true)


### 误差计算
提高迭代质量需要我们用一种方法来度量两个图像直接的误差，我们想输入图像和拟合图像的整体亮度没有关系，我们使用如下公式计算输入图像Input和拟合图像predict的误差。
$$\sum_{i,j}^{}\frac{input(i,j)}{\sum_{i,j}^{}{input(i,j)}}-\frac{predict(i,j)}{\sum_{i,j}^{}{predict(i,j)}}$$

### 细节
在实现中我们用CShader 和DDA算法进行连线生成图像，考虑到生成的图像是01二位值，在生成图像后用3x3高斯模糊来近似最后肉眼所见实际图像。误差的求和计算我们使用了一种简单的规约算法。本文所示图像是经过4800次成功的迭代后所呈现结果，即使所有的计算都在GPU中，整个迭代计算仍然花了4个小时。我们的画板是50cm长，计算机模拟画板分辨率是4096，每个线条的实际尺寸是50cm/4096 = 0.12mm。 我们在淘宝上找到一个0.1mm的黑色鱼线,共买了三卷共1500m鱼线进行缝制。

### 耗时
拟合结果 4小时
钉315个钉子 1小时
缝制4000个总共1300m鱼线线段：16小时


### 结果
![result](https://github.com/sienaiwun/Unity_Knit/blob/master/Images/result.jpg?raw=true)

### 代码仓库
[GitHub - sienaiwun/Unity_Knit](https://github.com/sienaiwun/Unity_Knit)

### 参考资料
[petros vrellis's knitting](http://artof01.com/vrellis/works/knit.html)
[Eric Heitz's Research Page](https://eheitzresearch.wordpress.com/)
