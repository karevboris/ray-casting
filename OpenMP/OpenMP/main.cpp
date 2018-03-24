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
#define deep 561
#define epsilon 0.0001

typedef unsigned char ubyte;

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

void initialClusters(){

}

int main(int argc, char **argv) {
	srand(time(0));

    color* pallete = new color[numClusters];

    ubyte* data = new ubyte[width*height*deep];

    for (int i = 0; i < numR; i++)
        for (int j = 0; j < numG; j++)
            for (int k = 0; k < numB; k++){
                color newColor(i*(numClusters / numR), j*(numClusters / numG), k*(numClusters / numB));
                pallete[i*numG*numB + j*numB + k] = newColor;
            }

    ubyte * memblock;

    FILE * file;
    fopen_s(&file, "rgb_big.raw", "r+");
    if (file == NULL) return -1;
    fseek(file, 0, SEEK_SET);
    int size = 3*width*height*deep;

    memblock = new ubyte[size];

    fread(memblock, sizeof(ubyte), size, file);
    fclose(file);

    ubyte indexR = 0, indexG = 0, indexB = 0;

    for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            for (int k = 0; k < deep; k++) {
                indexR = memblock[3 * (i*height*deep + j*deep + k)] / (numClusters / numR);
                indexG = memblock[3 * (i*height*deep + j*deep + k) + 1] / (numClusters / numG);
                indexB = memblock[3 * (i*height*deep + j*deep + k) + 2] / (numClusters / numB);
                data[i*height*deep + j*deep + k] = indexR*numG*numB + indexG*numB + indexB;
            }

    /*ubyte* resultPallete = new ubyte[3*numR*numG*numB];

    for (int i = 0; i < numR; i++)
        for (int j = 0; j < numG; j++)
            for (int k = 0; k < numB; k++){
                resultPallete[3 * (i*numG*numB + j*numB + k)] = pallete[i*numG*numB + j*numB + k].red;
                resultPallete[3 * (i*numG*numB + j*numB + k + 1)] = pallete[i*numG*numB + j*numB + k].green;
                resultPallete[3 * (i*numG*numB + j*numB + k + 2)] = pallete[i*numG*numB + j*numB + k].blue;
            }

    ofstream out("out.txt", ios::out | ios::binary);
    if (!out) {
        cout << "Cannot open file.\n";
        return 1;
    }
    out.write((unsigned char *)&resultPallete, 3 * numR*numG*numB);
    out.close();
    */

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

    /*
    fopen_s(&file, "out.txt", "r+");
    if (file == NULL) return -1;
    fseek(file, 0, SEEK_SET);
    size = 256*3;

    delete[] memblock;
    memblock = new ubyte[size];

    fread(memblock, sizeof(ubyte), size, file);
    fclose(file);
    */

    /*ifstream file("rgb_big.raw", ios::in | ios::binary | ios::ate);
    if (file.is_open())
    {
        size = file.tellg();
        memblock = new unsigned char[size];
        file.seekg(0, ios::beg);
        file.read(memblock, size);
        file.
        file.close();

        int count = 0;

        for (int i = 0; i < size; i++){
            if (memblock[i]!=0) count++;
        }

        cout << count;

        delete[] memblock;
    }
    else cout << "Unable to open file";
    */
    
    /*initialClusters();

    color* clusters = new color [numClusters];

    for (int i = 0; i < numClusters; i++) {
        color newColor(0, 0, 0, i);
        clusters[i] = newColor;
    }

    color* voxels = new color[width * height * deep];

    for (int i = 0; i < width; i++)
        for (int j = 0; j < height; j++)
            for (int k = 0; k < deep; k++){
                color newColor (0, 0, 0, 0);
                voxels[i] = newColor;
            }

    bool isMove = true;
    /*
    //while (isMove) {
        isMove = false;

#pragma omp parallel shared(voxels, clusters, isMove) num_threads(4) 
        {
#pragma omp for
            for (int i = 0; i < width * height * deep; i++){
            ubyte oldCluster = voxels[i].cluster;
            double min = metric(voxels[i], clusters[0]);
            int c;
            for (c = 1; c < numClusters; c++) {
                double newMetric = metric(voxels[i], clusters[c]);
                if (newMetric < min) {
                    min = newMetric;
                    voxels[i].cluster = c;
                    if (!isMove) isMove = true;
                }
            }
            if (i % 1000000==0) cout << i << endl;
        }
        }

        if (false) {
            for (ubyte c = 0; c < numClusters; c++) {
                int sumR = 0, sumG = 0, sumB = 0, count = 0;
                for (int i = 0; i < width * height * deep; i++){
                    if (voxels[i].cluster == c) {
                        sumR += voxels[i].red;
                        sumG += voxels[i].green;
                        sumB += voxels[i].blue;
                        count++;
                    }
                }
                clusters[c].red = (ubyte)(sumR / count);
                clusters[c].green = (ubyte)(sumG / count);
                clusters[c].blue = (ubyte)(sumB / count);
            }
        }
    //}

    delete[] voxels, clusters;

	/*double t1, t2, t3, t4, t5;
	int *set;
	edge *tree, *graph, *copy;
	edge **res;
	int size, m, proc;
	cin >> size;
	cin >> proc;
	m = size*(size - 1) / 2;

	int *ed = new int[proc];
	set = new int[size];
	graph = new edge[m];
	copy = new edge[m];
	tree = new edge[size - 1];
	res = new edge*[proc];
	for (int i = 0; i < proc; i++) res[i] = new edge[size - 1];
	
	

	for (int i = 0; i < size; i++)  set[i] = i;

	int k = 0;
	for (int i = 0; i < size - 1; i++)
		for (int j = i + 1; j < size; j++)
		{
			int w = rand() % size + 1;
			if (w > 0)
			{
				copy[k].x = graph[k].x = i;
				copy[k].y = graph[k].y = j;
				copy[k].w = graph[k].w = w;
				k++;
			}
		}
	int m_real = k;

	t1 = omp_get_wtime();

	int steps;
	if (m_real % 2 == 0)
		steps = m_real / 2 - 1;
	else
		steps = m_real / 2;
	for (int i = 0; i < m_real; i++)
	{
		if (i % 2 == 0)
		{
			for (int j = 0; j < m_real / 2; j++)
				if (graph[2 * j].w > graph[2 * j + 1].w)
				{
					edge tmp = graph[2 * j];
					graph[2 * j] = graph[2 * j + 1];
					graph[2 * j + 1] = tmp;
				}
		}
		else
			for (int j = 0; j < steps; j++)
				if (graph[2 * j + 1].w > graph[2 * j + 2].w)
				{
					edge tmp = graph[2 * j + 1];
					graph[2 * j + 1] = graph[2 * j + 2];
					graph[2 * j + 2] = tmp;
				}
	}
	
	t2 = omp_get_wtime();

	k = 0;
	int i = 0;
	while ((k<size - 1) && (i<m_real))
	{
		int x = set[graph[i].x];
		int y = set[graph[i].y];
		if (set[x] != set[y])
		{
			tree[k]=graph[i];
			k++;
			int z = (x < y) ? x : y;
				for (int j = 0; j < size; j++)
					if ((set[j] == x) || (set[j] == y)) set[j] = z;
		}
		i++;
	}
	int flag = 0;
	if (k != size - 1) flag = 1;
	
	if (!flag)
	{
		t3 = omp_get_wtime();

		for (int i = 0; i < m_real; i++)
		{
#pragma omp parallel shared (copy, i)
		{
			if (i % 2 == 0)
			{
#pragma omp for 
				for (int j = 0; j < m_real / 2; j++)
					if (copy[2 * j].w > copy[2 * j + 1].w)
					{
						edge tmp = copy[2 * j];
						copy[2 * j] = copy[2 * j + 1];
						copy[2 * j + 1] = tmp;
					}
			}
			else
#pragma omp  for 
				for (int j = 0; j < steps; j++)
					if (copy[2 * j + 1].w > copy[2 * j + 2].w)
					{
						edge tmp = copy[2 * j + 1];
						copy[2 * j + 1] = copy[2 * j + 2];
						copy[2 * j + 2] = tmp;
					}
		}
		}

		t4 = omp_get_wtime();

		k = 0;
		int i1;
#pragma omp parallel shared(copy) private(i1) num_threads(proc) 
		{
			int *set1 = new int[size];
			for (int j = 0; j < size; j++) set1[j] = j;
			int num = omp_get_thread_num();
			ed[num] = 0;

#pragma omp for
			for (i1 = 0; i1 < m_real; i1++)
				if (ed[num] < size - 1)
				{
					int x = set1[copy[i1].x];
					int y = set1[copy[i1].y];
					if (set1[x] != set1[y])
					{
						res[num][ed[num]] = copy[i1];
						ed[num]++;
						int z = (x < y) ? x : y;
						for (int j = 0; j < size; j++)
							if ((set1[j] == x) || (set1[j] == y)) set1[j] = z;
					}
				}
			if (num == 0)
			{
				for (int j = 0; j < size; j++) set[j] = set1[j];
			}
			delete[] set1;
		}

		for (int l = 1; l < proc; l++)
		{
			if (ed[0] == size - 1) break;
			i = 0;
			while ((ed[0] < size - 1) && (i < ed[l]))
			{
				int x = set[res[l][i].x];
				int y = set[res[l][i].y];
				if (set[x] != set[y])
				{
					res[0][ed[0]] = res[l][i];
					ed[0]++;
					int z = (x < y) ? x : y;
					for (int j = 0; j < size; j++)
						if ((set[j] == x) || (set[j] == y)) set[j] = z;
				}
				i++;
			}
		}

		t5 = omp_get_wtime();

		if (size < 11)
		{
			cout << "graph:" << endl;
			for (int i = 0; i < m_real; i++) cout << graph[i].x << " " << graph[i].y << " " << graph[i].w << endl;
			cout << "graph copy:" << endl;
			for (int i = 0; i < m_real; i++) cout << copy[i].x << " " << copy[i].y << " " << copy[i].w << endl;
			cout << "tree serial:" << endl;
			for (int i = 0; i < ed[0]; i++) cout << tree[i].x << " " << tree[i].y << " " << tree[i].w << endl;
			cout << "tree parallel:" << endl;
			for (int i = 0; i < ed[0]; i++) cout << res[0][i].x << " " << res[0][i].y << " " << res[0][i].w << endl;
		}

		cout << "num edge=" << m_real << endl;
		cout << "serial sort=" << (t2 - t1) << endl;
		cout << "parallel sort=" << (t4 - t3) << endl;
		cout << "serial=" << (t3 - t2) << endl;
		cout << "parallel=" << (t5 - t4) << endl;
		cout << "accel=" << (t3 - t2) / (t5 - t4) << endl;
		cout << "accel=" << (t3 - t1) / (t5 - t3) << endl;

		int sum1 = 0, sum2 = 0;
		for (int j = 0; j < size - 1; j++)
		{
			sum1 += tree[j].w;
			sum2 += res[0][j].w;
		}
		if (sum1 == sum2) cout << "true" << endl; else cout << "false" << endl;
	}
	else cout << "Uncorrect data" << endl;
	 
	delete [] set, graph, tree, ed, copy;
	for (int j = 0; j < proc; j++) delete [] res[j];
	delete[] res;

	getchar();
	getchar();*/

    getchar();
    delete[] memblock, data, pallete;
	return 0;
}