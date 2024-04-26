﻿namespace Shared.Dtos;

public class CreateConditionsLogDto
{ 
    public double SoilMoisturePercentage { get; set; }
    public double Light { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public long DeviceId { get; set; }
}