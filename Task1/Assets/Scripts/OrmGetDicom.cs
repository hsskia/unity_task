using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Mars.db;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Unity.VisualScripting.Antlr3.Runtime;
using System;
using UnityEngine.Rendering;
using System.Reflection;
using System.Data.Common;
using Newtonsoft.Json;
using System.Resources;

public class print_dicom_data : MonoBehaviour
{
    void PrintDicomStudy(JObject data){
        DicomStudy study = data.ToObject<DicomStudy>(); // 받아온 data를 DicomStudy class에 입력 
        string result = "Dicom Study Data \n";
        
        // study 클래스의 value들 출력
        foreach (var property in typeof(DicomStudy).GetProperties()){
            object val = property.GetValue(study);
            result += $"{property.Name}: {val} \n";
        }
        print(result);
    }

    void PrintDicomSeries(JArray data){
        int id_val = (int) data[0]["dicomStudyId"];
        string result = $"Dicom Study ID {id_val} Dicom Series Data \n";

        // study id에 일치하는 series 데이터들
        foreach (JObject item in data) {
            DicomSeries series = item.ToObject<DicomSeries>(); // 받아온 data를 DicomStudy class에 입력 
            string result2 = "----------------------------------------------------------------------------- \n";
            
            // series 데이터 출력
            foreach (var property in typeof(DicomSeries).GetProperties()){
                object val = property.GetValue(series);
                result2 += $"{property.Name}: {val} \n";
            }
            result += result2;
        }
        print(result);
    }
    void Start()
    {
        StartCoroutine(GetJsonData());
    }

    IEnumerator GetJsonData()
    {
        // http 접근 후 data GET
        string dicom_url = "http://10.10.20.173:5080/v2/Dicom/";

        UnityWebRequest req_study = UnityWebRequest.Get(dicom_url + "Study");
        yield return req_study.SendWebRequest();

        if (req_study.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(req_study.error);
        }
        else
        {
            // 서버에서 불러온 study 데이터를 string으로 변환 후 parsing을 위해 Json 형태로
            JArray dicom_study = JArray.Parse(req_study.downloadHandler.text);
            
            foreach (JObject item in dicom_study) {
                PrintDicomStudy(item);
                
                // Dicom stydy id에 일치하는 series data GET
                UnityWebRequest req_series = UnityWebRequest.Get(dicom_url + "Series?studyId="+ item["id"]);
                yield return req_series.SendWebRequest();
                
                if (req_series.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(req_series.error);
                }
                else
                {
                    JArray dicom_series = JArray.Parse(req_series.downloadHandler.text);
                    PrintDicomSeries(dicom_series);
                }
            }
        }
    }
}
