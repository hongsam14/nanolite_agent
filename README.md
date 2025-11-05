# Nanolite Agent

<p align="center">
  <img width="250" height="250" alt="Nanolite Agent Logo" src="https://github.com/user-attachments/assets/fb5d977a-0551-4ad3-88c3-118aa5d63af4" />
</p>

<p align="center">
  <strong>A lightweight Windows system monitoring agent that integrates Sysmon and OpenTelemetry for comprehensive system activity tracking</strong>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#requirements">Requirements</a> •
  <a href="#installation">Installation</a> •
  <a href="#configuration">Configuration</a> •
  <a href="#usage">Usage</a> •
  <a href="#architecture">Architecture</a> •
  <a href="#license">License</a>
</p>

---

## Overview

Nanolite Agent is a high-performance Windows monitoring agent designed to capture and export system activities using Event Tracing for Windows (ETW). It integrates with Sysmon for security event monitoring and uses OpenTelemetry Protocol (OTLP) to export telemetry data to observability platforms like Jaeger, Zipkin, or any OTLP-compatible collector.

The agent monitors critical system events including:
- Process creation and termination
- Thread activity
- Registry modifications
- Sysmon security events

## Features

- **Real-time Event Monitoring**: Captures Windows system events using ETW (Event Tracing for Windows)
- **Sysmon Integration**: Leverages Sysmon for advanced security monitoring and logging
- **OpenTelemetry Export**: Sends telemetry data to OTLP collectors for distributed tracing and analysis
- **Multiple Event Sessions**: 
  - Kernel Process Events
  - Kernel Thread Events
  - Kernel Registry Events
  - Sysmon Events
- **Lightweight & Efficient**: Minimal resource footprint with high-performance event processing
- **Self-Contained Deployment**: Single executable with all dependencies bundled
- **Graceful Shutdown**: Proper cleanup and flushing of events on termination

## Requirements

### System Requirements
- **Operating System**: Windows 10/11 or Windows Server 2016+
- **Platform**: x64 (64-bit)
- **.NET Runtime**: .NET 9.0 or later
- **Privileges**: Administrator rights (required for ETW session access)

### Prerequisites
- **Sysmon**: Microsoft Sysmon must be installed and running on the system
  - Download from [Microsoft Sysinternals](https://docs.microsoft.com/en-us/sysinternals/downloads/sysmon)
- **OTLP Collector**: A running OpenTelemetry collector or compatible backend (Jaeger, Zipkin, etc.)

## Installation

### Option 1: Build from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/hongsam14/nanolite_agent.git
   cd nanolite_agent
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build -c Release
   ```

4. **Publish as a self-contained executable**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

The compiled executable will be located in `bin/Release/net9.0/win-x64/publish/`.

### Option 2: Use Pre-built Binary

Download the latest release from the [Releases](https://github.com/hongsam14/nanolite_agent/releases) page.

## Configuration

The agent requires a `config.yml` file in the same directory as the executable. Create a configuration file with the following structure:

```yaml
CollectorIP: "localhost"           # IP address of your OTLP collector
CollectorPort: "4317"              # Port of your OTLP collector (default OTLP gRPC port)
Exporter: "your-service-name"      # Service name identifier for telemetry data
```

### Configuration Parameters

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `CollectorIP` | string | IP address or hostname of the OTLP collector | `localhost`, `192.168.1.100` |
| `CollectorPort` | string | Port number for the OTLP collector | `4317` (gRPC), `4318` (HTTP) |
| `Exporter` | string | Service name/identifier for the agent | `nanolite-agent-prod` |

### Example Configuration

```yaml
CollectorIP: "jaeger-collector.example.com"
CollectorPort: "4317"
Exporter: "nanolite-agent-server01"
```

## Usage

### Running the Agent

1. **Ensure Administrator Privileges**: The agent must run with administrator rights to access ETW sessions.

2. **Start the agent**:
   ```cmd
   nanolite-agent.exe
   ```

3. **Verify the agent is running**: You should see the Nanolite ASCII logo and initialization messages:
   ```
    ____    ____  ____    ___   _      ____  ______    ___ 
   |    \  /    ||    \  /   \ | |    |    ||      |  /  _]
   |  _  ||  o  ||  _  ||     || |     |  | |      | /  [_ 
   |  |  ||     ||  |  ||  O  || |___  |  | |_|  |_||    _]
   |  |  ||  _  ||  |  ||     ||     | |  |   |  |  |   [_ 
   |  |  ||  |  ||  |  ||     ||     | |  |   |  |  |     |
   |__|__||__|__||__|__| \___/ |_____||____|  |__|  |_____|
   c) 2025 Nanolite Agent by shhong ENKI Corp)
   ```

4. **Stop the agent**: Press `Ctrl + C` to gracefully stop monitoring and exit.

### Running as a Windows Service (Optional)

To run Nanolite Agent as a Windows service, you can use tools like [NSSM](https://nssm.cc/) (Non-Sucking Service Manager):

```cmd
nssm install NanoliteAgent "C:\path\to\nanolite-agent.exe"
nssm start NanoliteAgent
```

## Architecture

### Components

- **Event Sessions**: Multiple ETW session handlers for different event sources
  - `SysmonEventSession`: Captures Sysmon security events
  - `KernelProcessEventSession`: Monitors process lifecycle events
  - `KernelThreadEventSession`: Tracks thread creation and termination
  - `KernelRegistryEventSession`: Records registry modifications

- **System Activity Recorder**: Processes and formats captured events for export

- **Beacon**: Manages the connection and data transmission to the OTLP collector

### Data Flow

```
Windows Kernel/Sysmon Events 
    ↓
ETW Event Sessions
    ↓
System Activity Recorder
    ↓
OpenTelemetry Beacon
    ↓
OTLP Collector (Jaeger/Zipkin/etc.)
```

## Dependencies

The project uses the following key dependencies:

- **Microsoft.Diagnostics.Tracing.TraceEvent** (3.1.23): ETW event processing
- **OpenTelemetry** (1.12.0): Telemetry SDK
- **OpenTelemetry.Exporter.OpenTelemetryProtocol** (1.12.0): OTLP exporter
- **YamlDotNet** (16.3.0): YAML configuration parsing
- **Newtonsoft.Json** (13.0.3): JSON serialization

For a complete list of dependencies, see [nanolite-agent.csproj](nanolite-agent.csproj).

## Troubleshooting

### Common Issues

**Issue**: "Program must run with administrator privileges"
- **Solution**: Run the executable with administrator rights (right-click → Run as administrator)

**Issue**: Config file errors
- **Solution**: Ensure `config.yml` exists in the same directory as the executable and contains all required parameters

**Issue**: Cannot connect to OTLP collector
- **Solution**: Verify the collector is running and accessible at the configured IP and port

**Issue**: No Sysmon events captured
- **Solution**: Ensure Sysmon is installed and running (`sc query Sysmon64` or `sc query Sysmon`)

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the GNU General Public License v2.0 - see the [LICENSE](LICENSE) file for details.

## Author

**shhong** - ENKI Corp

© 2025 Nanolite Agent

## Acknowledgments

- Microsoft Sysinternals for [Sysmon](https://docs.microsoft.com/en-us/sysinternals/downloads/sysmon)
- [OpenTelemetry](https://opentelemetry.io/) project for observability standards
- [TraceEvent Library](https://github.com/microsoft/perfview) for ETW support
