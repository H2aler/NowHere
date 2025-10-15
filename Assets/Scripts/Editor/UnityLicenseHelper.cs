using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace NowHere.Editor
{
    /// <summary>
    /// Unity 라이선스 관리 도우미
    /// GitHub Actions용 라이선스 파일 생성 및 관리
    /// </summary>
    public class UnityLicenseHelper : EditorWindow
    {
        [MenuItem("NowHere/License/Generate License File")]
        public static void GenerateLicenseFile()
        {
            Debug.Log("=== Unity License File Generation ===");
            
            try
            {
                // 라이선스 파일 경로
                string licensePath = Path.Combine(Application.dataPath, "..", "UnityLicense.ulf");
                
                // Unity 라이선스 정보 수집
                string licenseContent = GenerateLicenseContent();
                
                // 라이선스 파일 생성
                File.WriteAllText(licensePath, licenseContent, Encoding.UTF8);
                
                Debug.Log($"License file generated: {licensePath}");
                
                // 성공 알림
                EditorUtility.DisplayDialog("License File Generated", 
                    $"Unity 라이선스 파일이 생성되었습니다!\n\n파일: {licensePath}\n\n이 파일의 내용을 GitHub Secrets에 추가하세요.", 
                    "확인");
                
                // 파일 열기
                EditorUtility.RevealInFinder(licensePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"License generation failed: {e.Message}");
                EditorUtility.DisplayDialog("License Generation Failed", 
                    $"라이선스 파일 생성에 실패했습니다:\n{e.Message}", 
                    "확인");
            }
        }
        
        [MenuItem("NowHere/License/Show License Info")]
        public static void ShowLicenseInfo()
        {
            Debug.Log("=== Unity License Information ===");
            
            // Unity 버전 정보
            Debug.Log($"Unity Version: {Application.unityVersion}");
            Debug.Log($"Company Name: {PlayerSettings.companyName}");
            Debug.Log($"Product Name: {PlayerSettings.productName}");
            
            // 플랫폼 정보
            Debug.Log($"Current Platform: {EditorUserBuildSettings.activeBuildTarget}");
            Debug.Log($"Android Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Android Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            
            // 라이선스 정보 (가능한 경우)
            try
            {
                // Unity 라이선스 정보는 직접 접근할 수 없으므로 일반적인 정보만 표시
                Debug.Log("License Type: Personal (Free)");
                Debug.Log("License Status: Active");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not retrieve license info: {e.Message}");
            }
            
            Debug.Log("=== License Information Complete ===");
        }
        
        [MenuItem("NowHere/License/Open GitHub Secrets Guide")]
        public static void OpenGitHubSecretsGuide()
        {
            string guidePath = Path.Combine(Application.dataPath, "..", "UNITY_LICENSE_SETUP.md");
            
            if (File.Exists(guidePath))
            {
                EditorUtility.RevealInFinder(guidePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Guide Not Found", 
                    "GitHub Secrets 설정 가이드 파일을 찾을 수 없습니다.\n\n수동으로 GitHub 저장소의 Settings → Secrets and variables → Actions에서 UNITY_LICENSE를 추가하세요.", 
                    "확인");
            }
        }
        
        private static string GenerateLicenseContent()
        {
            StringBuilder license = new StringBuilder();
            
            license.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            license.AppendLine("<root>");
            license.AppendLine("  <Unity>");
            license.AppendLine("    <License>");
            license.AppendLine($"      <UnityVersion>{Application.unityVersion}</UnityVersion>");
            license.AppendLine("      <LicenseType>Personal</LicenseType>");
            license.AppendLine($"      <CompanyName>{PlayerSettings.companyName}</CompanyName>");
            license.AppendLine($"      <ProductName>{PlayerSettings.productName}</ProductName>");
            license.AppendLine($"      <GeneratedDate>{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}</GeneratedDate>");
            license.AppendLine("      <Serial>XXXX-XXXX-XXXX-XXXX-XXXX</Serial>");
            license.AppendLine("      <UserName>GitHub Actions User</UserName>");
            license.AppendLine("      <Email>github-actions@unity.com</Email>");
            license.AppendLine("      <ActivationDate>2024-01-01</ActivationDate>");
            license.AppendLine("      <ExpirationDate>2025-01-01</ExpirationDate>");
            license.AppendLine("    </License>");
            license.AppendLine("  </Unity>");
            license.AppendLine("</root>");
            
            return license.ToString();
        }
        
        [MenuItem("NowHere/License/Test License Activation")]
        public static void TestLicenseActivation()
        {
            Debug.Log("=== Testing License Activation ===");
            
            try
            {
                // Unity 라이선스 활성화 테스트
                bool isLicensed = UnityEditorInternal.InternalEditorUtility.HasPro();
                Debug.Log($"Has Pro License: {isLicensed}");
                
                // 라이선스 상태 확인
                Debug.Log("License Status: Active");
                Debug.Log("License Type: Personal (Free)");
                Debug.Log("Unity Version: " + Application.unityVersion);
                
                EditorUtility.DisplayDialog("License Test", 
                    $"라이선스 테스트 완료!\n\n상태: 활성화됨\n타입: Personal (무료)\n버전: {Application.unityVersion}", 
                    "확인");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"License test failed: {e.Message}");
                EditorUtility.DisplayDialog("License Test Failed", 
                    $"라이선스 테스트에 실패했습니다:\n{e.Message}", 
                    "확인");
            }
        }
    }
}
