# 🚀 Deploy to Render.com - FREE Full-Stack Hosting!

Deploy your Form Maker app (Blazor WASM + API) completely **FREE** using:
- **Frontend**: GitHub Pages (already set up!)
- **Backend API**: Render.com (100% free!)

## ✅ What You Get (FREE)

- ✅ 750 hours/month free (enough for 24/7 hosting!)
- ✅ Automatic HTTPS
- ✅ Automatic deployments from GitHub
- ✅ PostgreSQL database (optional, also free!)
- ✅ No credit card required for free tier

## 📋 Prerequisites

- GitHub account (you have this!)
- Render.com account (free - create at https://render.com)

---

## 🔧 Step-by-Step Setup

### 1. Create Render Account

1. Go to https://render.com
2. Click **"Get Started"**
3. Sign up with your **GitHub account** (easiest!)
4. Authorize Render to access your repositories

### 2. Deploy the API

1. In Render Dashboard, click **"New +"** → **"Web Service"**

2. **Connect Repository**:
   - Select your `form-maker` repository
   - Click **"Connect"**

3. **Configure Service**:
   - **Name**: `formmaker-api`
   - **Region**: Choose closest to you
   - **Branch**: `main`
   - **Root Directory**: `FormMaker.Api`
   - **Runtime**: `Docker`
   - **Instance Type**: `Free`

4. **Build Settings**:
   - Render will auto-detect the Dockerfile!
   - Build Command: `dotnet publish -c Release`

5. **Environment Variables** (Click "Advanced"):
   Add these environment variables:
   ```
   FUNCTIONS_WORKER_RUNTIME = dotnet-isolated
   AzureWebJobsStorage = UseDevelopmentStorage=false
   JwtSecret = (click "Generate" to create a secure random value)
   JwtIssuer = FormMaker
   JwtAudience = FormMakerClient
   ```

6. Click **"Create Web Service"**

### 3. Wait for Deployment

- First deploy takes ~5-10 minutes
- Watch the logs in real-time
- When you see "Your service is live 🎉", it's ready!

### 4. Get Your API URL

After deployment:
1. Copy your service URL (looks like: `https://formmaker-api.onrender.com`)
2. **This is your API URL!** 

### 5. Update Your Client Configuration

1. Open `FormMaker.Client/wwwroot/appsettings.json`
2. Update the `ApiBaseUrl`:
   ```json
   {
     "ApiBaseUrl": "https://formmaker-api.onrender.com/api/"
   }
   ```
   Replace `formmaker-api` with your actual Render service name!

### 6. Deploy Frontend to GitHub Pages

Your frontend is already on GitHub Pages! Just push your changes:

```bash
git add .
git commit -m "Configure Render.com API deployment"
git push origin main
```

GitHub Actions will automatically update your site at:
**https://zyonify.github.io/form-crafter/**

---

## 🧪 Testing Your Live App

### Test Backend API:
Visit: `https://your-api-url.onrender.com/api/auth/login`

Should return: "Method Not Allowed" or similar (means it's working!)

### Test Full App:
1. Go to https://zyonify.github.io/form-crafter/
2. Click **"Sign Up"**
3. Create an account
4. You should be automatically logged in!
5. Create a template in the editor
6. Save it - **it's now in the cloud!** ☁️

---

## 📊 Monitor Your App

### Render Dashboard:
- **Logs**: Real-time application logs
- **Metrics**: CPU, Memory, Request stats
- **Events**: Deployment history

### Check if API is sleeping:
Free tier spins down after 15 min of inactivity.
- First request after sleep: ~30 seconds
- Subsequent requests: Instant!

---

## 🔒 Database Setup (Optional)

Want persistent database instead of SQLite?

### Add PostgreSQL (FREE):

1. In Render Dashboard: **"New +"** → **"PostgreSQL"**
2. Name: `formmaker-db`
3. Instance Type: **Free**
4. Click **"Create Database"**

5. In your API service **Environment Variables**:
   - Find the **Internal Database URL**
   - Update `ConnectionStrings__FormMakerDb` to use it

6. Update `FormMaker.Api/Program.cs`:
   ```csharp
   // Change from UseSqlite to UseNpgsql
   options.UseNpgsql(connectionString);
   ```

7. Add NuGet package:
   ```bash
   dotnet add FormMaker.Api package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

---

## 🛠️ Troubleshooting

### API Not Responding?
- Check Render logs for errors
- Verify environment variables are set
- API might be sleeping (first request takes 30s)

### CORS Errors?
- Check `staticwebapp.config.json` has correct CORS headers
- Verify API URL in `appsettings.json` is correct

### Authentication Not Working?
- Verify `JwtSecret` is set in Render environment
- Check browser console for errors
- Ensure API URL ends with `/api/`

### Database Errors?
- Check SQLite file permissions
- Consider upgrading to PostgreSQL (free!)

---

## 💰 Costs

### Current Setup: $0/month
- **Render API**: Free (750 hours/month)
- **GitHub Pages**: Free (unlimited)
- **PostgreSQL**: Free (1GB storage)

### If You Exceed Free Tier:
- Render will notify you
- Upgrade to Starter ($7/month) if needed
- **But for personal use, free tier is plenty!**

---

## 🚀 Next Steps

1. ✅ Custom domain for GitHub Pages (free with GitHub)
2. ✅ Custom domain for Render API ($0 extra)
3. ✅ Set up monitoring/alerts in Render
4. ✅ Add PostgreSQL for production data

---

## 📚 Resources

- [Render.com Documentation](https://render.com/docs)
- [Azure Functions on Docker](https://docs.microsoft.com/azure/azure-functions/functions-create-function-linux-custom-image)
- [GitHub Pages Docs](https://docs.github.com/pages)

---

## 🎉 Success!

Your app is now:
- ✅ **100% cloud-hosted**
- ✅ **Completely free**
- ✅ **Automatically deployed**
- ✅ **Globally accessible**
- ✅ **Secured with HTTPS**

**Live URLs:**
- Frontend: https://zyonify.github.io/form-crafter/
- Backend: https://your-service-name.onrender.com/api/

**Congratulations!** 🎊 You now have a production full-stack application!
