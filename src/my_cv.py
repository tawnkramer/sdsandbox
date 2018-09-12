'''
file: ddqn.py
author: Felix Yu
date: 2018-09-12
original: https://raw.githubusercontent.com/flyyufelix/donkey_rl/master/donkey_rl/src/my_cv.py
'''
import cv2
import numpy as np

def remove_noise(image, kernel_size):
    return cv2.GaussianBlur(image, (kernel_size, kernel_size), 0)


def discard_colors(image):
    return cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)


def detect_edges(image, low_threshold, high_threshold):
    return cv2.Canny(image, low_threshold, high_threshold)


def draw_lines(image, lines, color=[255, 0, 0], thickness=2):
    for line in lines:
        for x1,y1,x2,y2,slope in line:
            x1, y1, x2, y2 = int(x1),int(y1),int(x2),int(y2)
            cv2.line(image, (x1, y1), (x2, y2), color, thickness)


def hough_lines(image, rho, theta, threshold, min_line_len, max_line_gap):
    lines = cv2.HoughLinesP(image, rho, theta, threshold, np.array([]), minLineLength=min_line_len, maxLineGap=max_line_gap)
    return lines


def slope(x1, y1, x2, y2):
    try:
        return (y1 - y2) / (x1 - x2)
    except:
        return 0
        

def separate_lines(lines):
    right = []
    left = []

    if lines is not None:
        for x1,y1,x2,y2 in lines[:, 0]:
            m = slope(x1,y1,x2,y2)
            if m >= 0:
                right.append([x1,y1,x2,y2,m])
            else:
                left.append([x1,y1,x2,y2,m])
    return left, right


def reject_outliers(data, cutoff, threshold=0.08, lane='left'):
    data = np.array(data)
    data = data[(data[:, 4] >= cutoff[0]) & (data[:, 4] <= cutoff[1])]
    try:
        if lane == 'left':
            return data[np.argmin(data,axis=0)[-1]]
        elif lane == 'right':
            return data[np.argmax(data,axis=0)[-1]]
    except:
        return []


def extend_point(x1, y1, x2, y2, length):
    line_len = np.sqrt((x1 - x2)**2 + (y1 - y2)**2)
    x = x2 + (x2 - x1) / line_len * length
    y = y2 + (y2 - y1) / line_len * length
    return x, y
