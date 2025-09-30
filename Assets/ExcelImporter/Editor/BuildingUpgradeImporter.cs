using UnityEngine;
using UnityEditor;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;

public class BuildingUpgradeImporter : Editor
{
    private static string XLSX_PATH = "Assets/Excel/BuildingUpgradeSO.xlsx";

    [MenuItem("My Tools/Import BuildingUpgrade Data")]
    public static void Import()
    {
        var so = CreateInstance<BuildingUpgradeSO>();

        using (FileStream stream = File.Open(XLSX_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))//공부하기
        {
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheet("BuildingUpgrade");

            // 데이터가 시작되는 행 번호
            int startRow = 1;

            for (int row = startRow; row <= sheet.LastRowNum; row++)
            {
                IRow currentRow = sheet.GetRow(row);
                if (currentRow == null || currentRow.GetCell(0) == null) continue;

                var data = new BuildingUpgradeData();

                // 기본 정보 읽기
                data.idNumber = GetSafeInt(currentRow, 0);
                data.buildingName = GetSafeString(currentRow, 1);
                data.level = GetSafeInt(currentRow, 2);
                data.nextLevel = GetSafeInt(currentRow, 3);

                if (data.nextLevel == -1)
                {
                    Debug.Log($"행 {row + 1} (ID: {data.idNumber}): nextLevel이 비어있으므로 최대 레벨입니다.");
                }

                // 비용(Cost) 데이터 읽기 (최대 4쌍)
                for (int i = 0; i < 4; i++)
                {
                    int typeColIndex = 4 + (i * 2);
                    int amountColIndex = 5 + (i * 2);

                    string costTypeStr = GetSafeString(currentRow, typeColIndex);
                    if (!string.IsNullOrEmpty(costTypeStr))
                    {
                        var cost = new Cost();
                        cost.resourceType = (ResourceType)System.Enum.Parse(typeof(ResourceType), costTypeStr);
                        cost.amount = GetSafeInt(currentRow, amountColIndex);
                        data.costs.Add(cost);
                    }
                }

                // 효과(Effect) 데이터 읽기 (최대 4쌍)
                for (int i = 0; i < 4; i++)
                {
                    int typeColIndex = 12 + (i * 3);
                    int minColIndex = 13 + (i * 3);
                    int maxColIndex = 14 + (i * 3);

                    string effectTypeStr = GetSafeString(currentRow, typeColIndex);
                    if (!string.IsNullOrEmpty(effectTypeStr))
                    {
                        var effect = new BuildingEffect();
                        effect.effectType = (BuildingEffectType)System.Enum.Parse(typeof(BuildingEffectType), effectTypeStr);
                        effect.effectValueMin = GetSafeFloat(currentRow, minColIndex);
                        effect.effectValueMax = GetSafeFloat(currentRow, maxColIndex);
                        data.effects.Add(effect);
                    }
                }
                data.description = GetSafeString(currentRow, 24);
                int buildingTypeColIndex = 25;
                string buildingTypeStr = GetSafeString(currentRow, buildingTypeColIndex);

                // 엑셀에 값이 비어있지 않은 경우에만 Enum으로 변환을 시도합니다.
                if (!string.IsNullOrEmpty(buildingTypeStr))
                {
                    // 엑셀에 적힌 문자열(예: "Farm")을 실제 BuildingType Enum 값으로 변환합니다.
                    try
                    {
                        data.buildingType = (BuildingType)System.Enum.Parse(typeof(BuildingType), buildingTypeStr, true); // true: 대소문자 무시
                    }
                    catch (System.ArgumentException)
                    {
                        // 엑셀에 오타가 있을 경우를 대비한 경고 메시지
                        Debug.LogError($"행 {row + 1}: BuildingType '{buildingTypeStr}'을(를) 찾을 수 없습니다. 오타가 있는지 확인해주세요.");
                        data.buildingType = BuildingType.None;
                    }
                }
                else
                {
                    data.buildingType = BuildingType.None;
                }
                int spritePathColIndex = 26; 
                string spritePath = GetSafeString(currentRow, spritePathColIndex);
                if (!string.IsNullOrEmpty(spritePath))
                {
                    // Resources.Load<Sprite>()를 사용하여 경로의 스프라이트를 불러옵니다.
                    data.buildingSprite = Resources.Load<Sprite>(spritePath);

                    if (data.buildingSprite == null)
                    {
                        Debug.LogWarning($"행 {row + 1}: 스프라이트 로드 실패! 경로를 확인하세요: Resources/{spritePath}");
                    }
                }
                so.BuildingUpgrade.Add(data);
            }
        }
        string assetPath = "Assets/Resources/DB/BuildingUpgradeSO.asset";
        if (File.Exists(assetPath)) AssetDatabase.DeleteAsset(assetPath);

        AssetDatabase.CreateAsset(so, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("BuildingUpgrade 데이터 임포트 완료! " + assetPath);
    }

    // --- 셀 값을 안전하게 가져오는 헬퍼 함수들 ---
    private static string GetSafeString(IRow row, int cellIndex)
    {
        ICell cell = row.GetCell(cellIndex);
        return cell?.ToString() ?? "";
    }

    private static int GetSafeInt(IRow row, int cellIndex)
    {
        ICell cell = row.GetCell(cellIndex);
        if (cell == null || cell.CellType != CellType.Numeric) return -1;
        return (int)cell.NumericCellValue;

    }

    private static float GetSafeFloat(IRow row, int cellIndex)
    {
        ICell cell = row.GetCell(cellIndex);
        if (cell == null || cell.CellType != CellType.Numeric) return -1f;
        return (float)cell.NumericCellValue;
    }
}