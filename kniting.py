# -*- coding: utf-8 -*-
import os
import math
import sys
import random
import argparse
from PIL import Image, ImageDraw, ImageFilter

def convertBinary(im, threshold=30):
    Lim = im.convert('L')
    table = []
    for i in range(256):
        if i < threshold:
            table.append(0)
        else:
            table.append(1)
    bim = Lim.point(table, '1')
    return bim

def getLinePoints(startPoint, endPoint):
    result = []
    x0 = startPoint[0]
    y0 = startPoint[1]
    x1 = endPoint[0]
    y1 = endPoint[1]
    dx = x1 - x0
    dy = y1 - y0
    if (abs(dy) > abs(dx)):
        steps = abs(dy)
    else:
        steps = abs(dx)
    if steps == 0:
        return result
    xinc = float(dx) / steps
    yinc = float(dy) / steps
    result.append((x0, y0))
    x = x0
    y = y0
    for i in xrange(int(steps)):
        x += xinc
        y += yinc
        if (x, y) not in result:
            result.append((int(x), int(y)))
    return result

def getAvgLight(source, lines, edges_im=None):
    if len(lines) == 0:
        return sys.maxint
    light = 0
    for key in lines:
        x, y = key
        cubeKeys = [
            (x - 1, y - 1),
            (x + 1, y - 1),
            (x - 1, y + 1),
            (x + 1, y + 1),
            (x, y + 1),
            (x + 1, y),
            (x, y - 1),
            (x - 1, y),
        ]
        cubeKeys.append(key)
        l = 0
        cubeKeys = filter(lambda k: k in source, cubeKeys)
        for k in cubeKeys:
            l += source[k]
            if edges_im is not None:
                edge_value = edges_im.getpixel(k)
                if edge_value > 10:
                    l += 40
        if cubeKeys:
            light += (l / len(cubeKeys))
    light = light / len(lines)
    return light

def getLowLight(source, lines, th=80):
    if len(lines) == 0:
        return 0
    count = 0
    for key in lines:
        if source.get(key, 0) < th:
            count += 1
    return count

def getAvgColor(im, lines):
    if len(lines) == 0:
        return 0, 0, 0
    color = (0, 0, 0)
    n = 0
    for key in lines:
        x, y = key
        if x < 0 or x >= im.width or y < 0 or y >= im.height:
            continue
        n += 1
        pixel = im.getpixel(key)
        if isinstance(pixel, int):
            color += (pixel, pixel, pixel)
        else:
            color += pixel
    if n == 0:
        return 0, 0, 0
    color = (color[0] / n, color[1] / n, color[2] / n)
    return color


def getLineLight(source, pinBuffer, step=200):
    resLight = None
    resIndex = (0, 0)
    pinNum = len(pinBuffer)

    while step > 0:
        i = random.randint(0, pinNum - 1)
        j = random.randint(0, pinNum - 1)
        if i == j:
            continue
        lines = getLinePoints(pinBuffer[i], pinBuffer[j])
        light = getAvgLight(source, lines)
        count = getLowLight(source, lines)
        campValue = (count, light)
        if campValue < resLight or resLight is None:
            resLight = campValue
            resIndex = (i, j)
        step -= 1

    return resIndex

def findPath(source, startIndex, pinBuffer, edges_im=None):
    resIndexs = []
    pinNum = len(pinBuffer)
    for i in xrange(pinNum):
        if i == startIndex:
            continue
        lines = getLinePoints(pinBuffer[startIndex], pinBuffer[i])
        light = getAvgLight(source, lines, edges_im)
        resIndexs.append((light, i))
    resIndexs.sort(key=lambda item: item[0])
    resIndexs = resIndexs[0:10]
    resIndex = random.choice(resIndexs)[1]
    lines = getLinePoints(pinBuffer[startIndex], pinBuffer[resIndex])
    for point in lines:
        x = int(point[0])
        y = int(point[1])
        key = (x, y)
        if key not in source:
            continue
        source[key] += 10
    return resIndex

if __name__ == '__main__':

    parser = argparse.ArgumentParser()
    parser.add_argument("--input", default='input.png', help="The input image filename")
    parser.add_argument("--output", default="result.png", help="The output image filename")
    parser.add_argument("--path", default="path.csv", help="The path result filename")
    parser.add_argument("--pin-num", default=200, help="The number of pin", type=int)
    parser.add_argument("--path-num", default=1000, help="The number of path", type=int)
    args = parser.parse_args()

    inputFilename = args.input
    outputFilename = args.output
    pathFilename = args.path

    pinNum = 200
    pathNum = 2000

    im = Image.open(inputFilename)
    edges_im = im.filter(ImageFilter.FIND_EDGES)
    width, height = im.width, im.height

    pinBuffer = []
    lineBuff = []
    deltaAngle = 2 * math.pi / pinNum
    radius = width * 0.5
    lradius = 500
    for i in xrange(pinNum):
        x = radius + radius * math.sin(i * deltaAngle)
        y = radius + radius * math.cos(i * deltaAngle)
        pinBuffer.append((int(x), int(y)))
        x = lradius + lradius * math.sin(i * deltaAngle)
        y = lradius + lradius * math.cos(i * deltaAngle)
        lineBuff.append((int(x), int(y)))

    source = {}
    for x in xrange(width):
        for y in xrange(height):
            pixel = im.getpixel((x, y))
            if isinstance(pixel, int):
                source[(x, y)] = pixel
            else:
                r, g, b = pixel
                source[(x, y)] = (r * 0.3 + g * 0.59 + b * 0.11)

    percent = 0
    startIndex = 0
    nextIndex = -1
    result = Image.new('RGB', (lradius * 2, lradius * 2), 'rgb(255,255,255)')
    draw = ImageDraw.Draw(result)
    f = open(pathFilename, 'wb')
    for i in xrange(pathNum):
        if i == 0:
            startIndex, nextIndex = getLineLight(source, pinBuffer)
        else:
            nextIndex = findPath(source, startIndex, pinBuffer, edges_im=edges_im)
        x1, y1 = lineBuff[startIndex]
        x2, y2 = lineBuff[nextIndex]
        lines = getLinePoints(lineBuff[startIndex], lineBuff[nextIndex])
        color = getAvgColor(im, lines)
        color = 'rgb(%d, %d, %d)' % color
        draw.line((x1, y1, x2, y2), fill=color, width=1)
        startIndex = nextIndex
        currentP = int(float(i) / pathNum * 100)
        f.write('%d,' % startIndex)
        if currentP != percent:
            percent = currentP
            try:
                result.save(outputFilename)
                os.system('cls')
                print 'process: %d%% step: %d' % (percent, i)
            except:
                sys.excepthook(*sys.exc_info())
    if nextIndex != -1:
        f.write('%d,' % nextIndex)
    f.close()
    result.save(outputFilename)
