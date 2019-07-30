# Unity_Knit

This project is made for my first wedding ceremoy. I use Unity to made an implementation of [petros vrellis's knitting](http://artof01.com/vrellis/works/knit.html). The method is inspired by [Eric Heitz's Research Page](https://eheitzresearch.wordpress.com/)

![input](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/input.jpg)
![prediction](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/prediction2.jpg)
![realization](https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/realization2.jpg)
![closeup1]
(https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/closeup1.jpg)
![closeup2]
(https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/closeup2.jpg)
![wip]
(https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/wip.jpg)
![sequence]
(https://github.com/sienaiwun/Unity_Knit/blob/master/Assets/MatImages/sequence.jpg)

Input parameters:
Input image, Nails number, knitting path number, cavas resolution.

We set the cavas resolution to be 2048. The cavase and fish line are bought from Taobao.
The cavas is 50 cm in diameter and 157 cm in perimeters. The nails is knocked 5 mm each. Fish line is 0.12mm in diameter and 1500m long.

### 中文
这项工程是我和我老婆的结婚第一年生日礼物，是用线缝纫出来的我们的双人头像。
这项工程的输入参数有头像照片，钉子数目，缝纫线端长度和画布尺寸。
材料是由淘宝购得，分别是50cm直径圆形画板，0.12mm 黑色鱼线1500m，和钉子若干。

### 实施步骤。
第一步是钉钉，我们在网上买的画板直径157cm,我们用5毫米的距离钉钉子，这样的间距是精度和工作量的一个很好的平衡，这样一共315个钉子。我们用4k个线段来模拟输入图像。本工程很更具输入的参数进行线段模拟，通过误差较真的方法生成拟合线段，文本输出模拟结果。最好更具模拟输出结果联系好即为最后作品。
