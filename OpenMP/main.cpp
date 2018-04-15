#include <iostream>
#include <fstream>
#include <ctime>
#include <string>
#include <vector>
#include <iterator>
#include <omp.h>

#define numClusters 256
#define numR 4
#define numG 8
#define numB 8
#define width 920
#define height 840
#define deep 10
#define epsilon 0.0001

typedef unsigned char ubyte;
typedef unsigned short ushort;

using namespace std;

struct color {
    ubyte red;
    ubyte green;
    ubyte blue;
    color(){}
    color(ubyte r, ubyte g, ubyte b) {
        red = r;
        green = g;
        blue = b;
    }
};

double metric(color x, color y){
    return sqrt(pow(x.red - y.red, 2) + pow(x.green - y.green, 2) + pow(x.blue - y.blue, 2));
}

void RGBtoHSV(ubyte r, ubyte g, ubyte b, float &H, float &S, float &V) {
    float R = (float)r / 255;
    float G = (float)g / 255;
    float B = (float)b / 255;

    float max = R, min = R;
    if (max < G) max = G; if (max < B) max = B;
    if (min > G) min = G; if (min > B) min = B;

    float diff = max - min;

    if (diff == 0) H = 0;
    else  if (max == R){
        if (G >= B) H = 60 * (float)(G - B) / diff;
        else H = 60 * (float)(G - B) / diff + 360;
    }
    else if (max == G) H = 60 * (B - R) / diff + 120;
    else H = 60 * (R - G) / diff + 240;

    if (max == 0) S = 0;
    else S = 1 - min / max;

    V = max;
}

void HSVtoRGB(float H, float S, float V, ubyte &r, ubyte &g, ubyte &b){
    V *= 100;
    S *= 100;

    float R = 0, G = 0, B = 0;

    float V_min = (100 - S)*V / 100;
    float a = (V - V_min)*(float)((int)H / 60) / 60;
    float V_inc = V_min + a;
    float V_dec = V - a;

    switch ((int)H / 6)
    {
    case 0: R = V; G = V_inc; B = V_min; break;
    case 1: R = V_dec; G = V; B = V_min; break;
    case 2: R = V_min; G = V; B = V_inc; break;
    case 3: R = V_min; G = V_dec; B = V; break;
    case 4: R = V_inc; G = V_min; B = V; break;
    case 5: R = V; G = V_min; B = V_dec; break;
    default: break;
    }

    r = (ubyte)(R * 255 / 100);
    g = (ubyte)(G * 255 / 100);
    b = (ubyte)(B * 255 / 100);
}

int getIndex(float H){
    if (H == 360) return numClusters-1;
    return (int)(H / 1.40625);
}

int main(int argc, char **argv) {
    srand(time(0));

    int size = 3 * width*height*deep;

    ubyte* memblock = new ubyte[size];

    color* pallete = new color[numClusters];

    ubyte* data = new ubyte[width*height*deep];

    /*for (int i = 0; i < numR; i++)
        for (int j = 0; j < numG; j++)
        for (int k = 0; k < numB; k++){
        color newColor(i*(numClusters / numR), j*(numClusters / numG), k*(numClusters / numB));
        pallete[i*numG*numB + j*numB + k] = newColor;
        }

        */

    FILE * file;
    fopen_s(&file, "rgb_big.raw", "r+");
    if (file == NULL) return -1;
    fseek(file, 0, SEEK_SET);

    fread(memblock, sizeof(ubyte), size, file);
    fclose(file);

    ubyte indexR = 0, indexG = 0, indexB = 0;

    ubyte R = 0, G = 0, B = 0;

    double sumErr = 0, maxErr = 0;

    double t1 = 0, t2 = 0;

    long * sumColorR = new long[numClusters];
    long * sumColorG = new long[numClusters];
    long * sumColorB = new long[numClusters];

    int * numberColors = new int[numClusters];

    for (int i = 0; i < numClusters; i++){
        color newColor(rand() % 255, rand() % 255, rand() % 255);
        pallete[i] = newColor;
        numberColors[i] = sumColorR[i] = sumColorG[i] = sumColorB[i] = 0;
    }
    
    int iter = 0;

    t1 = omp_get_wtime();

    while (iter<1)
    {
#pragma omp parallel num_threads(4)
    {
#pragma omp for
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                for (int k = 0; k < deep; k++){
                    R = memblock[3 * (i*height*deep + j*deep + k)];
                    G = memblock[3 * (i*height*deep + j*deep + k) + 1];
                    B = memblock[3 * (i*height*deep + j*deep + k) + 2];

                    color sourceColor(R, G, B);

                    double min = metric(pallete[0], sourceColor);
                    ubyte argmin = 0;
                    for (int l = 1; l < numClusters; l++){
                        double current = metric(pallete[l], sourceColor);
                        if (current < min) {
                            min = current;
                            argmin = l;
                        }
                    }
                    data[i*height*deep + j*deep + k] = argmin;
                    sumColorR[argmin] += R;
                    sumColorG[argmin] += G;
                    sumColorB[argmin] += B;
                    numberColors[argmin]++;
                }
    }
        for (int i = 0; i < numClusters; i++){
            if (numberColors[i] != 0){
                color newColor((ubyte)(sumColorR[i] / numberColors[i]), (ubyte)(sumColorG[i] / numberColors[i]), (ubyte)(sumColorB[i] / numberColors[i]));
                pallete[i] = newColor;
            }
            numberColors[i] = sumColorR[i] = sumColorG[i] = sumColorB[i] = 0;
        }
        iter++;
    }

    t2 = omp_get_wtime();

    sumErr = maxErr = 0;

    for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            for (int k = 0; k < deep; k++) {
                color sourceColor(memblock[3 * (i*height*deep + j*deep + k)], memblock[3 * (i*height*deep + j*deep + k) + 1], memblock[3 * (i*height*deep + j*deep + k) + 2]);
                color searchColor = pallete[data[i*height*deep + j*deep + k]];

                double err = metric(sourceColor, searchColor);
                if (maxErr < err) maxErr = err;
                sumErr += err;
            }

    cout << "Max error 3: " << maxErr << endl;
    cout << "Average error 3: " << sumErr / (width*height*deep) << endl;
    cout << "Time: " << t2 - t1 << endl;


    delete[] sumColorR, sumColorG, sumColorB, numberColors;

    /*t1 = omp_get_wtime();

//#pragma omp parallel num_threads(4)
    {
//#pragma omp for reduction(+:sumErr)
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                for (int k = 0; k < deep; k++) {
                    color sourceColor(memblock[3 * (i*height*deep + j*deep + k)], memblock[3 * (i*height*deep + j*deep + k) + 1], memblock[3 * (i*height*deep + j*deep + k) + 2]);
                    color searchColor = pallete[indexR*numG*numB + indexG*numB + indexB];

                    indexR = sourceColor.red / (numClusters / numR);
                    indexG = sourceColor.green / (numClusters / numG);
                    indexB = sourceColor.blue / (numClusters / numB);
                    data[i*height*deep + j*deep + k] = indexR*numG*numB + indexG*numB + indexB;

                    double err = metric(sourceColor, searchColor);
                    if (maxErr < err) maxErr = err;
                    sumErr += err;
                }
    }

    t2 = omp_get_wtime();

    cout << "Max error 1: " << maxErr << endl;
    cout << "Average error 1: " << sumErr / (width*height*deep) << endl;
    cout << "Time: " << t2 - t1 << endl;

    fopen_s(&file, "out_hsv.raw", "r+");
    if (file == NULL) return -1;
    fseek(file, 0, SEEK_SET);

    fread(pallete, sizeof(ubyte), numClusters, file);
    fread(data, sizeof(ubyte), size / 3, file);
    fclose(file);

    sumErr = maxErr = 0;

    for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            for (int k = 0; k < deep; k++) {
                color sourceColor(memblock[3 * (i*height*deep + j*deep + k)], memblock[3 * (i*height*deep + j*deep + k) + 1], memblock[3 * (i*height*deep + j*deep + k) + 2]);
                color searchColor = pallete[data[i*height*deep+j*deep+k]];

                double err = metric(sourceColor, searchColor);
                if (maxErr < err) maxErr = err;
                sumErr += err;
            }

    cout << "Max error 2: " << maxErr << endl;
    cout << "Average error 2: " << sumErr / (width*height*deep) << endl;

    /*
    ofstream fout("out.raw");
    
    for (int i = 0; i < numR; i++)
        for (int j = 0; j < numG; j++)
            for (int k = 0; k < numB; k++){
                fout << pallete[i*numG*numB + j*numB + k].red << pallete[i*numG*numB + j*numB + k].green << pallete[i*numG*numB + j*numB + k].blue;
            }

    for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            for (int k = 0; k < deep; k++) {
                fout << data[i*height*deep + j*deep + k];
            }

    fout.close();

    delete[] data, pallete;

    pallete = new color[numClusters];

    data = new ubyte[width*height*deep];

    float *H = new float[numClusters];
    float *S = new float[numClusters];
    float *V = new float[numClusters];
    int *number = new int[numClusters];

    for (int i = 0; i < numClusters; i++){
        H[i] = S[i] = V[i] = number[i] = 0;
    }

    ubyte R = 0, G = 0, B = 0;
    float h = 0, s = 0, v = 0;


    t1 = omp_get_wtime();

    for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            for (int k = 0; k < deep; k++) {
                R = memblock[3 * (i*height*deep + j*deep + k)];
                G = memblock[3 * (i*height*deep + j*deep + k) + 1];
                B = memblock[3 * (i*height*deep + j*deep + k) + 2];
                
                RGBtoHSV(R, G, B, h, s, v);

                int index = getIndex(h);
                H[index] += h;
                S[index] += s;
                V[index] += v;
                number[index]++;
            }

    for (int i = 0; i < numClusters; i++){
        int num = number[i];
        if (num != 0){
            H[i] /= num;
            S[i] /= num;
            V[i] /= num;
        }

        R = 0, G = 0, B = 0;
        HSVtoRGB(H[i], S[i], V[i], R, G, B);
        pallete[i] = color(R, G, B);
    }

    delete[] H, S, V, number;

    sumErr = 0;

#pragma omp parallel num_threads(4)
    {
#pragma omp for reduction(+:sumErr)
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                for (int k = 0; k < deep; k++){
                    R = memblock[3 * (i*height*deep + j*deep + k)];
                    G = memblock[3 * (i*height*deep + j*deep + k) + 1];
                    B = memblock[3 * (i*height*deep + j*deep + k) + 2];
                    color tmpColor(R, G, B);
                    double err = 0;
                    double minErr = metric(tmpColor, pallete[0]);
                    data[i*height*deep + j*deep + k] = 0;
                    for (int l = 1; l < numClusters; l++) {
                        err = metric(tmpColor, pallete[l]);
                        if (err < minErr) {
                            minErr = err;
                            data[i*height*deep + j*deep + k] = l;
                        }
                    }

                    sumErr += minErr;
                }

    }

    t2 = omp_get_wtime();

    cout << "Time: " << t2 - t1 << endl;
    cout << "Average error 2: " << sumErr / (width*height*deep) << endl;

    ofstream fout_hsv("out_hsv.raw");

    for (int i = 0; i < numR; i++)
        for (int j = 0; j < numG; j++)
            for (int k = 0; k < numB; k++){
                fout_hsv << pallete[i*numG*numB + j*numB + k].red << pallete[i*numG*numB + j*numB + k].green << pallete[i*numG*numB + j*numB + k].blue;
            }

    for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            for (int k = 0; k < deep; k++) {
                fout_hsv << data[i*height*deep + j*deep + k];
            }

    fout_hsv.close();
    */

    getchar();
    delete[] memblock, data, pallete;
	return 0;
}