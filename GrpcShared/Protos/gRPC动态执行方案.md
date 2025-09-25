# gRPC动态执行方案

好的，這是一份從零開始，涵蓋環境準備、專案設定、程式碼編寫到最終執行的完整解決方案。方案中的每一步都盡可能詳細，您可以依此進行程式碼的編寫和部署。

### 專案目標

我們要建立一個 Java 專案，該專案能夠：

1. ​**自動化**：在執行前，透過程式碼自動將 `.proto` 檔案編譯成 Protobuf 描述符檔案 (`.dsc`)。
2. ​**動態化**：在執行時，動態地讀取 `.dsc` 檔案，無需依賴靜態生成的 Java gRPC 類別，即可發送請求。
3. ​**通用化**：能夠根據傳入的參數（服務名、方法名、JSON 請求體、驗證規則）執行任何 gRPC 呼叫並進行結果驗證。
4. ​**自包含**：專案內建一個 gRPC 伺服器用於測試，無需依賴外部服務。

---

### 第〇步：環境準備

在開始編寫程式碼之前，請確保您的開發環境和將要部署的伺服器環境滿足以下條件：

1. ​**Java Development Kit (JDK)** ：版本 8 或更高。
2. ​**Maven**：用於專案管理和依賴管理。
3. ​**Protocol Buffers Compiler (**​**​`protoc`​**​ **)** ：​**這是唯一的外部工具依賴**。請務必安裝。

    - ​**Linux (Ubuntu/Debian)** : `sudo apt install -y protobuf-compiler`​
    - ​**macOS (Homebrew)** : `brew install protobuf`​
    - ​**Windows**: 從 [Protobuf GitHub Releases](https://github.com/protocolbuffers/protobuf/releases) 下載並將 `protoc.exe` 所在路徑加入系統 `Path` 環境變數。

---

### 第一步：建立 Maven 專案與設定

1. ​**建立專案**：使用您的 IDE 或 Maven 命令建立一個新的 Java 專案。
2. ​**專案結構**：請依照以下結構建立資料夾和檔案：
3. ​**​`pom.xml`​**​ **設定檔**：這是專案的核心設定，包含了所有依賴和一個重要的外掛程式（用於為**測試伺服器**生成程式碼）。
4. ​**放置**  **​`.proto`​**​ **檔案**：將您提供的 `device_status.proto` 檔案複製到 `src/main/proto/device_status.proto` 路徑下。

---

### 第二步：編寫程式碼

現在，我們來填充專案結構中的 Java 檔案。

#### 2.1 `DescriptorGenerator.java` (自動化描述符生成器)

這個類別負責在執行前呼叫 `protoc` 命令，生成我們的動態客戶端所依賴的 `.dsc` 檔案。

Java

```
package com.example.dynamicgrpc;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.concurrent.TimeUnit;

public class DescriptorGenerator {
    public static Path generateDescriptor(String protoFilePath, String outputDir)
            throws IOException, InterruptedException {
        File protoFile = new File(protoFilePath);
        if (!protoFile.exists()) {
            throw new IOException("Proto file not found: " + protoFilePath);
        }

        Path outDirPath = Paths.get(outputDir);
        Files.createDirectories(outDirPath);
        
        String dscFileName = protoFile.getName().replace(".proto", ".dsc");
        Path dscFilePath = outDirPath.resolve(dscFileName);

        ProcessBuilder processBuilder = new ProcessBuilder(
            "protoc",
            "--descriptor_set_out=" + dscFilePath.toAbsolutePath().toString(),
            "--proto_path=" + protoFile.getParentFile().getAbsolutePath(), // 指定 proto 檔案的根目錄
            "--include_imports",
            protoFile.getAbsolutePath()
        );

        System.out.println("Executing command: " + String.join(" ", processBuilder.command()));
        Process process = processBuilder.start();

        if (!process.waitFor(30, TimeUnit.SECONDS)) {
            process.destroy();
            throw new IOException("protoc command timed out.");
        }

        int exitCode = process.exitValue();
        if (exitCode == 0) {
            System.out.println("✅ Descriptor file generated successfully at: " + dscFilePath);
            return dscFilePath;
        } else {
            String errorOutput = new java.util.Scanner(process.getErrorStream()).useDelimiter("\\A").next();
            throw new IOException("protoc command failed with exit code " + exitCode + ":\n" + errorOutput);
        }
    }
}
```

#### 2.2 `server/MockDeviceStatusServer.java` (用於測試的伺服器)

這個伺服器使用**靜態生成**的類別（由 `protobuf-maven-plugin` 生成），為我們的動態客戶端提供一個可以呼叫的目標。

Java

```
package com.example.dynamicgrpc.server;

import device.status.v1.*; // 這是由 Maven 外掛程式靜態生成的類別
import io.grpc.Server;
import io.grpc.inprocess.InProcessServerBuilder;
import io.grpc.stub.StreamObserver;

import java.io.IOException;

public class MockDeviceStatusServer {
    private final Server server;
    public MockDeviceStatusServer(String serverName) throws IOException {
        this.server = InProcessServerBuilder.forName(serverName).directExecutor()
                .addService(new DeviceStatusServiceImpl())
                .build()
                .start();
    }

    public void stop() throws InterruptedException {
        if (server != null) {
            server.shutdown().awaitTermination();
        }
    }

    private static class DeviceStatusServiceImpl extends DeviceStatusServiceGrpc.DeviceStatusServiceImplBase {
        @Override
        public void getDeviceStatus(DeviceRequest request, StreamObserver<DeviceResponse> responseObserver) {
            System.out.println("[Server] Received request for device: " + request.getDeviceId());
            DeviceResponse.Builder responseBuilder = DeviceResponse.newBuilder()
                    .setDeviceId(request.getDeviceId())
                    .setDeviceName("Dynamic Test Device")
                    .setDeviceType(DeviceType.GATEWAY)
                    .setStatus(DeviceStatus.ACTIVE)
                    .setFirmwareVersion("v2.1.0")
                    .setLastActiveTimestamp(System.currentTimeMillis() / 1000L);

            if (request.getIncludeMetrics()) {
                DeviceResponse.PerformanceMetrics metrics = DeviceResponse.PerformanceMetrics.newBuilder()
                        .setCpuUsage(45.5)
                        .setMemoryUsage(60.2)
                        .setTemperature(38.7)
                        .setNetworkThroughput(120.5)
                        .build();
                responseBuilder.setMetrics(metrics);
            }
            responseObserver.onNext(responseBuilder.build());
            responseObserver.onCompleted();
        }
    }
}
```

*注意：在您第一次編寫此文件時，IDE 可能會報錯找不到* *​`device.status.v1.*`​* ​ *的類別。請執行一次 Maven 編譯 (*​*​`mvn compile`​*​ *)，*​*​`protobuf-maven-plugin`​*​ *會自動生成這些類別，之後錯誤就會消失。*

#### 2.3 `AdvancedDynamicGrpcClient.java` (核心動態客戶端)

這個客戶端是我們解決方案的核心，負責動態地處理 gRPC 呼叫。

Java

```
// 此處程式碼與上一回答中的 AdvancedDynamicGrpcClient.java 相同，請直接複製使用
package com.example.dynamicgrpc;

import com.google.protobuf.DescriptorProtos;
import com.google.protobuf.Descriptors;
import com.google.protobuf.DynamicMessage;
import com.google.protobuf.util.JsonFormat;
import io.grpc.CallOptions;
import io.grpc.ManagedChannel;
import io.grpc.MethodDescriptor;
import io.grpc.StatusRuntimeException;
import io.grpc.protobuf.ProtoUtils;
import io.grpc.stub.ClientCalls;

import java.io.FileInputStream;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Map;
import java.util.Objects;

public class AdvancedDynamicGrpcClient {
    private final ManagedChannel channel;
    private final Map<String, Descriptors.ServiceDescriptor> serviceDescriptorMap = new HashMap<>();

    public AdvancedDynamicGrpcClient(ManagedChannel channel, String descriptorFilePath) throws Exception {
        this.channel = channel;
        // 注意：這裡改為從檔案系統讀取，而不是從 resource
        try (InputStream dscStream = new FileInputStream(descriptorFilePath)) {
            DescriptorProtos.FileDescriptorSet descriptorSet = DescriptorProtos.FileDescriptorSet.parseFrom(dscStream);
            for (DescriptorProtos.FileDescriptorProto fdp : descriptorSet.getFileList()) {
                Descriptors.FileDescriptor fileDescriptor = Descriptors.FileDescriptor.buildFrom(fdp, new Descriptors.FileDescriptor[]{});
                for (Descriptors.ServiceDescriptor service : fileDescriptor.getServices()) {
                    serviceDescriptorMap.put(service.getFullName(), service);
                }
            }
        }
        System.out.println("✅ Client Initialized. Available services: " + serviceDescriptorMap.keySet());
    }

    public DynamicMessage callRpc(String fullServiceName, String methodName, String jsonRequest) throws Exception {
        Descriptors.ServiceDescriptor serviceDescriptor = serviceDescriptorMap.get(fullServiceName);
        if (serviceDescriptor == null) throw new IllegalArgumentException("Service not found: " + fullServiceName);
        
        Descriptors.MethodDescriptor methodDescriptor = serviceDescriptor.findMethodByName(methodName);
        if (methodDescriptor == null) throw new IllegalArgumentException("Method not found: " + methodName);

        Descriptors.Descriptor requestDescriptor = methodDescriptor.getInputType();
        Descriptors.Descriptor responseDescriptor = methodDescriptor.getOutputType();

        DynamicMessage.Builder requestBuilder = DynamicMessage.newBuilder(requestDescriptor);
        JsonFormat.parser().ignoringUnknownFields().merge(jsonRequest, requestBuilder);
        DynamicMessage requestMessage = requestBuilder.build();

        MethodDescriptor<DynamicMessage, DynamicMessage> dynamicMethodDescriptor =
                MethodDescriptor.<DynamicMessage, DynamicMessage>newBuilder()
                        .setType(MethodDescriptor.MethodType.UNARY)
                        .setFullMethodName(MethodDescriptor.generateFullMethodName(fullServiceName, methodName))
                        .setRequestMarshaller(ProtoUtils.marshaller(requestMessage))
                        .setResponseMarshaller(ProtoUtils.marshaller(DynamicMessage.getDefaultInstance(responseDescriptor)))
                        .build();
        
        System.out.println("🚀 Calling " + dynamicMethodDescriptor.getFullMethodName());
        System.out.println(">>> Request JSON: " + jsonRequest);

        try {
            return ClientCalls.blockingUnaryCall(channel, dynamicMethodDescriptor, CallOptions.DEFAULT, requestMessage);
        } catch (StatusRuntimeException e) {
            System.err.println("RPC call failed: " + e.getStatus());
            throw e;
        }
    }
}
```

#### 2.4 `GenericValidator.java` (通用驗證器)

這個驗證器負責根據動態傳入的規則來檢查響應是否符合預期。

Java

```
// 此處程式碼與上一回答中的 GenericValidator.java 相同，請直接複製使用
package com.example.dynamicgrpc;

import com.google.protobuf.Descriptors;
import com.google.protobuf.DynamicMessage;
import java.util.Map;
import java.util.Objects;

public class GenericValidator {
    public static void validate(DynamicMessage responseMessage, Map<String, Object> expectations) {
        System.out.println("\n--- Starting Generic Validation ---");
        for (Map.Entry<String, Object> entry : expectations.entrySet()) {
            String fieldPath = entry.getKey();
            Object expectedValue = entry.getValue();
            try {
                Object actualValue = getFieldByPath(responseMessage, fieldPath);
                
                if (actualValue instanceof Descriptors.EnumValueDescriptor) {
                    actualValue = ((Descriptors.EnumValueDescriptor) actualValue).getName();
                }

                if (Objects.deepEquals(actualValue, expectedValue)) {
                    System.out.printf("[PASS] '%s': Expected [%s], Actual [%s]\n", fieldPath, expectedValue, actualValue);
                } else {
                    System.out.printf("[FAIL] '%s': Expected [%s], but got [%s]\n", fieldPath, expectedValue, actualValue);
                }
            } catch (Exception e) {
                System.out.printf("[FAIL] '%s': Error while validating - %s\n", fieldPath, e.getMessage());
            }
        }
        System.out.println("--- Validation Finished ---\n");
    }

    private static Object getFieldByPath(DynamicMessage message, String path) {
        String[] parts = path.split("\\.");
        Object currentValue = message;
        for (int i = 0; i < parts.length; i++) {
            if (!(currentValue instanceof DynamicMessage)) {
                throw new IllegalArgumentException("Cannot find path part '" + parts[i] + "' because parent is not a message.");
            }
            DynamicMessage currentMessage = (DynamicMessage) currentValue;
            Descriptors.Descriptor currentDescriptor = currentMessage.getDescriptorForType();
            Descriptors.FieldDescriptor fieldDescriptor = currentDescriptor.findFieldByName(parts[i]);

            if (fieldDescriptor == null) {
                throw new IllegalArgumentException("Field '" + parts[i] + "' not found in message " + currentDescriptor.getName());
            }
            currentValue = currentMessage.getField(fieldDescriptor);
        }
        return currentValue;
    }
}
```

#### 2.5 `DynamicClientRunner.java` (總執行器)

這是整個專案的入口點，它會按順序協調所有步驟，從生成描述符到最終驗證。

Java

```
package com.example.dynamicgrpc;

import com.example.dynamicgrpc.server.MockDeviceStatusServer;
import com.google.protobuf.DynamicMessage;
import com.google.protobuf.util.JsonFormat;
import io.grpc.ManagedChannel;
import io.grpc.inprocess.InProcessChannelBuilder;

import java.nio.file.Path;
import java.util.HashMap;
import java.util.Map;

public class DynamicClientRunner {

    public static void main(String[] args) throws Exception {
        // --- 1. 自動化步驟: 生成描述符檔案 ---
        String protoPath = "src/main/proto/device_status.proto";
        String descriptorDir = "target/descriptors";
        Path dscPath = DescriptorGenerator.generateDescriptor(protoPath, descriptorDir);

        // --- 2. 準備測試環境: 啟動記憶體伺服器 ---
        String serverName = "in-process-server-for-dynamic-test";
        MockDeviceStatusServer server = new MockDeviceStatusServer(serverName);
        ManagedChannel channel = InProcessChannelBuilder.forName(serverName).directExecutor().build();

        try {
            // --- 3. 定義動態參數 ---
            String serviceToCall = "device.status.v1.DeviceStatusService";
            String methodToCall = "GetDeviceStatus";
            String requestJson = "{\n" +
                    "  \"device_id\": \"a7b3c1d9-0e8f-4a2b-8c6d-5e4f3a2b1c0d\",\n" +
                    "  \"include_metrics\": true\n" +
                    "}";
            
            Map<String, Object> validationRules = new HashMap<>();
            validationRules.put("device_id", "a7b3c1d9-0e8f-4a2b-8c6d-5e4f3a2b1c0d");
            validationRules.put("status", "ACTIVE");
            validationRules.put("metrics.cpu_usage", 45.5);
            validationRules.put("device_type", "GATEWAY");

            // --- 4. 執行核心邏輯 ---
            // 傳入 dsc 檔案的路徑來初始化客戶端
            AdvancedDynamicGrpcClient client = new AdvancedDynamicGrpcClient(channel, dscPath.toString());
            
            // 發起 RPC 呼叫
            DynamicMessage response = client.callRpc(serviceToCall, methodToCall, requestJson);

            // 將響應轉為 JSON 格式並印出，方便偵錯
            String responseJson = JsonFormat.printer().print(response);
            System.out.println("<<< Response JSON:\n" + responseJson);

            // 根據規則驗證響應
            GenericValidator.validate(response, validationRules);

        } finally {
            // --- 5. 清理資源 ---
            channel.shutdownNow();
            server.stop();
            System.out.println("Execution finished.");
        }
    }
}
```

---

### 第三步：執行與驗證

1. ​**編譯專案**：打開終端，進入專案根目錄，執行 Maven 編譯命令。這一步會為我們的測試伺服器生成必要的類別。
2. ​**執行主程式**：透過您的 IDE 執行 `DynamicClientRunner.java` 的 `main` 方法，或者使用 Maven 命令執行。

**預期輸出結果：**

您應該會在主控台看到類似下面的輸出，展示了從生成描述符到最終驗證成功的完整流程。

```
Executing command: protoc --descriptor_set_out=.../target/descriptors/device_status.dsc --proto_path=.../src/main/proto --include_imports .../src/main/proto/device_status.proto
✅ Descriptor file generated successfully at: .../target/descriptors/device_status.dsc
✅ Client Initialized. Available services: [device.status.v1.DeviceStatusService]
🚀 Calling device.status.v1.DeviceStatusService/GetDeviceStatus
>>> Request JSON: {
  "device_id": "a7b3c1d9-0e8f-4a2b-8c6d-5e4f3a2b1c0d",
  "include_metrics": true
}
[Server] Received request for device: a7b3c1d9-0e8f-4a2b-8c6d-5e4f3a2b1c0d
<<< Response JSON:
{
  "deviceId": "a7b3c1d9-0e8f-4a2b-8c6d-5e4f3a2b1c0d",
  "deviceName": "Dynamic Test Device",
  "deviceType": "GATEWAY",
  "status": "ACTIVE",
  "lastActiveTimestamp": "...",
  "firmwareVersion": "v2.1.0",
  "metrics": {
    "cpuUsage": 45.5,
    "memoryUsage": 60.2,
    "temperature": 38.7,
    "networkThroughput": 120.5
  }
}

--- Starting Generic Validation ---
[PASS] 'device_id': Expected [a7b3c1d9-0e8f-4a2b-8c6d-5e4f3a2b1c0d], Actual [a7b3c1d9-0e8f-4a2b-8c6d-5e4f3a2b1c0d]
[PASS] 'status': Expected [ACTIVE], Actual [ACTIVE]
[PASS] 'metrics.cpu_usage': Expected [45.5], Actual [45.5]
[PASS] 'device_type': Expected [GATEWAY], Actual [GATEWAY]
--- Validation Finished ---

Execution finished.
```

這份完整的解決方案為您提供了一個可立即執行的範本，您可以基於此進行擴充，例如從設定檔讀取呼叫參數和驗證規則，以應用到您的實際專案中。
